
using ContratacaoService.Application.Repositories;
using Microsoft.Data.SqlClient;
using PropostaService.Application.Repositories;
using PropostaService.Application.Services;
using PropostaService.Infrastructure.Data;
using System.Data;

namespace PropostaService.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var connectionString = builder.Configuration.GetConnectionString("PropostaDb");

            builder.Services.AddTransient<IDbConnection>(_ =>
                new SqlConnection(connectionString));

            builder.Services.AddScoped<IPropostaRepository, PropostaRepository>();
            builder.Services.AddScoped<IPropostaAppService, PropostaAppService>();

            builder.Services.AddScoped<IContratacaoRepository, ContratacaoRepository>();
            builder.Services.AddScoped<IContratacaoAppService, ContratacaoAppService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            

            app.MapControllers();

            app.Run();

            
        }
    }
}
