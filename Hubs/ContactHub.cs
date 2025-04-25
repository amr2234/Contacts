using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace UsersContacts.Hubs
{
    public class ContactHub : Hub
    {
        private static readonly ConcurrentDictionary<int, string> _lockedContacts = new();

        public async Task LockContact(int contactId)
        {
            try
            {
                if (!IsValidConnection(out var connectionId))
                {
                    await SendErrorToCallerAsync("Invalid connection");
                    return;
                }

                if (_lockedContacts.TryGetValue(contactId, out var currentLockOwner))
                {
                    if (currentLockOwner == connectionId)
                    {
                        await Clients.Caller.SendAsync("ContactLocked", contactId);
                        return;
                    }
                    await SendErrorToCallerAsync("This contact is currently being edited by another user. Please try again later.");
                    return;
                }

                if (_lockedContacts.TryAdd(contactId, connectionId))
                {
                    await Clients.All.SendAsync("ContactLocked", contactId);
                }
                else
                {
                    await SendErrorToCallerAsync("Failed to lock contact. Please try again.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in LockContact: {ex}");
                await SendErrorToCallerAsync("An error occurred while locking the contact");
            }
        }

        public async Task UnlockContact(int contactId)
        {
            if (!IsValidConnection(out var connectionId))
            {
                await SendErrorToCallerAsync("Invalid connection");
                return;
            }

            if (_lockedContacts.TryGetValue(contactId, out var lockOwner) && lockOwner == connectionId)
            {
                await RemoveLockAndNotifyAsync(contactId);
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (IsValidConnection(out var connectionId))
            {
                var locksToRelease = _lockedContacts
                    .Where(x => x.Value == connectionId)
                    .Select(x => x.Key)
                    .ToList();

                foreach (var contactId in locksToRelease)
                {
                    await RemoveLockAndNotifyAsync(contactId);
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task UpdateContact(int contactId, string field, string value)
        {
            if (!IsValidConnection(out var connectionId))
            {
                await SendErrorToCallerAsync("Invalid connection");
                return;
            }

            if (!_lockedContacts.TryGetValue(contactId, out var lockOwner) || lockOwner != connectionId)
            {
                await SendErrorToCallerAsync("You can't edit this contact");
                return;
            }

            await Clients.All.SendAsync("ContactUpdated", contactId, field, value);
        }

        private async Task SendErrorToCallerAsync(string message)
        {
            await Clients.Caller.SendAsync("Error", message);
        }

        private async Task RemoveLockAndNotifyAsync(int contactId)
        {
            _lockedContacts.TryRemove(contactId, out _);
            await Clients.All.SendAsync("ContactUnlocked", contactId);
        }

        private bool IsValidConnection(out string connectionId)
        {
            connectionId = Context.ConnectionId;
            return !string.IsNullOrEmpty(Context.ConnectionId);
        }
    }
}