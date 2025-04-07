using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using backend.Models;

namespace backend.Algorithms
{
    public class JWTImplementation
    {
        // para poder acceder a la configuracion de appsettings.json
        private readonly IConfiguration _configuration;
        public JWTImplementation(IConfiguration configuration)
        {
            _configuration = configuration;

        }

        public string generarJWT(User modelo)
        {
            //Creamos la informacion para el token
            var userClaims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, modelo.id.ToString()), // para poder identificar al usuario por su id
                new Claim(ClaimTypes.Email, modelo.email!), 

            };

            //Creamos la clave para el token
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]!));
            var credential = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            //Crear el detalle del token
            var jwtConfig = new JwtSecurityToken(
                claims: userClaims,
                expires: DateTime.UtcNow.AddHours(1), // Expira en 1 hora
                signingCredentials: credential
                );
            return new JwtSecurityTokenHandler().WriteToken(jwtConfig);


        }

    }

}
