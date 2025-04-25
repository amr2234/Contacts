using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UsersContacts.Mappers;
using UsersContacts.Modules.Contacts.Commands.CreateContact;
using UsersContacts.Modules.Contacts.Commands.DeleteContact;
using UsersContacts.Modules.Contacts.Commands.UpdateContact;
using UsersContacts.Modules.Contacts.DTOs;
using UsersContacts.Modules.Contacts.Queries.GetContact;
using UsersContacts.Modules.Contacts.Queries.GetPaginatedContacts;

namespace UsersContacts.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ContactController(IMediator mediator) : Controller
    {
        private readonly IMediator _mediator = mediator;

        [HttpGet]
        public async Task<IActionResult> Index(string? search = "", int pageNumber = 1, int pageSize = 5)
        {
            var query = new GetPaginatedContactsQuery
            {
                Search = search,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var (contacts, totalCount) = await _mediator.Send(query);
            ViewBag.TotalCount = totalCount;
            ViewBag.PageSize = pageSize;
            ViewBag.PageNumber = pageNumber;
            ViewBag.Search = search;

            return View(contacts);
        }

        [HttpGet("api")]
        public async Task<IActionResult> GetContacts(string? search = "", int pageNumber = 1, int pageSize = 5)
        {
            var query = new GetPaginatedContactsQuery
            {
                Search = search,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var (contacts, totalCount) = await _mediator.Send(query);

            return Ok(new
            {
                contacts = contacts.Select(c => c.ToEntity()).ToList(),
                totalCount,
                currentPage = pageNumber,
                totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ContactDto contact)
        {
            if (ModelState.IsValid)
            {
                var command = new CreateContactCommand { Contact = contact };
                await _mediator.Send(command);
                return Ok();
            }
            return BadRequest(ModelState);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ContactDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var command = new UpdateContactCommand
            {
                Id = id,
                Contact = request
            };

            await _mediator.Send(command);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var command = new DeleteContactCommand { Id = id };
            var result = await _mediator.Send(command);

            if (!result) return NotFound();
            return Ok();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var query = new GetContactQuery { Id = id };
            var contact = await _mediator.Send(query);

            if (contact == null) return NotFound();
            return Ok(contact);
        }
    }
}