using Application.Errors;
using Application.Interfaces;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Application.User
{
	public class Login
    {
        public class Query : IRequest<User>
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }

        public class QueryValidator : AbstractValidator<Query>
        {
            public QueryValidator()
            {
                RuleFor(x => x.Email).NotEmpty();
                RuleFor(x => x.Password).NotEmpty();
            }
        }

		public class Handler : IRequestHandler<Query, User>
		{
			private readonly IJwtGenerator _jwtGenerator;
			private readonly UserManager<AppUser> _userManager;
			private readonly SignInManager<AppUser> _signInManger;

			public Handler(UserManager<AppUser> userManager, 
				SignInManager<AppUser> signInManger, IJwtGenerator jwtGenerator)
			{
				_jwtGenerator = jwtGenerator;
				_userManager = userManager;
				_signInManger = signInManger;
			}

			public async Task<User> Handle(Query request, CancellationToken cancellationToken)
			{
				var user = await _userManager.FindByEmailAsync(request.Email);

				if (user == null)
					throw new RestException(HttpStatusCode.Unauthorized);

				var result = await _signInManger.CheckPasswordSignInAsync(user, request.Password, false);

				if (result.Succeeded)
				{
					return new User
					{
						DisplayName = user.DisplayName,
						Token = _jwtGenerator.CreateToken(user),
						Username = user.UserName,
						Image = null
					};
				}

				throw new RestException(HttpStatusCode.Unauthorized);
			}
		}
	}
}