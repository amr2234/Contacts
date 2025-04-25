using MediatR;
using UsersContacts.Interfaces;
using UsersContacts.Mappers;
using UsersContacts.Modules.Contacts.DTOs;

namespace UsersContacts.Modules.Contacts.Queries.GetPaginatedContacts
{
    public class GetPaginatedContactsQuery : IRequest<(IEnumerable<ContactDto> Contacts, int TotalCount)>
    {
        public string? Search { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 5;
    }

    public class GetPaginatedContactsQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetPaginatedContactsQuery, (IEnumerable<ContactDto> Contacts, int TotalCount)>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<(IEnumerable<ContactDto> Contacts, int TotalCount)> Handle(GetPaginatedContactsQuery request, CancellationToken cancellationToken)
        {
            var (contacts, totalCount) = await _unitOfWork.ContactRepository.GetPagedAsync(
                search: request.Search,
                pageNumber: request.PageNumber,
                pageSize: request.PageSize
            );

            return (contacts.Select(c => c.ToDto()), totalCount);
        }
    }
}