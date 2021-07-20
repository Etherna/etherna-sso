using Etherna.SSOServer.Domain.Models;
using System;

namespace Etherna.SSOServer.Areas.Api.DtoModels
{
    public class UserContactInfoDto
    {
        // Constructor.
        public UserContactInfoDto(UserBase user)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            Email = user.Email;
            PhoneNumber = user.PhoneNumber;
        }

        // Properties.
        public string? Email { get; }
        public string? PhoneNumber { get; }
    }
}
