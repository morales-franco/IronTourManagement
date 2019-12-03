using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Linq;
using TourManagement.API.Services;

namespace TourManagement.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(setupAction =>
            {
                //setupAction.ReturnHttpNotAcceptable = true;

                var jsonOutputFormatter = setupAction.OutputFormatters
                .OfType<JsonOutputFormatter>().FirstOrDefault();

                /*TODO: support output formatter of custom media types - when we send information - GET
                 * When a Client retrieve information, it needs to specify the custom media type:
                 * for example: api/tour/1 accept: application/vnd.iron.tour+json
                 * If we don't declare this custom media type, the API will return 406:NOT ACCEPTABLE 
                 *      ==> this is for the configuration: setupAction.ReturnHttpNotAcceptable = true;
                 * If we declare the custom media type, the operation will be successful.
                 * 
                 * The client can specify the best media type for him with: accept: application/vnd.iron.tour+json. In this case
                 * the cliend tell us "application/vnd.iron.tour+json" I really would like that you return to me the data in this format. The API try to achieve that.
                 * 1) configuration: setupAction.ReturnHttpNotAcceptable = true; AND application/vnd.iron.tour+json Not declared ==> Return error 406 - not acceptable
                 * 2) configuration: setupAction.ReturnHttpNotAcceptable = false; AND application/vnd.iron.tour+json Not declared ==> Return 200 but the content-type=application/json
                 *      ==> by default an api net core try to return the data in the client preference format but if this format is not available, return JSON
                 * 3) configuration: setupAction.ReturnHttpNotAcceptable = true/false; AND application/vnd.iron.tour+json is declared ==> Return 200 but the content-type=application/vnd.iron.tour+json
                */
                if (jsonOutputFormatter != null)
                {
                    jsonOutputFormatter.SupportedMediaTypes
                    .Add("application/vnd.iron.tour+json");

                    jsonOutputFormatter.SupportedMediaTypes
                    .Add("application/vnd.iron.tourwithestimatedprofits+json");

                    jsonOutputFormatter.SupportedMediaTypes
                    .Add("application/vnd.iron.tourwithshows+json");

                    jsonOutputFormatter.SupportedMediaTypes
                    .Add("application/vnd.iron.tourwithestimatedprofitsandshows+json");

                    jsonOutputFormatter.SupportedMediaTypes
                   .Add("application/vnd.iron.tourwithshowsforcreation+json");

                    jsonOutputFormatter.SupportedMediaTypes
                    .Add("application/vnd.iron.tourwithmanagerandshowsforcreation+json");

                    jsonOutputFormatter.SupportedMediaTypes
                   .Add("application/vnd.iron.showcollection+json");
                }

                var jsonInputFormatter = setupAction.InputFormatters
                .OfType<JsonInputFormatter>().FirstOrDefault();

                /*
                 * TODO: support input formatter of custom media types - when we receive information - POST
                 * The client specifies the format of the data (body data) in the Content-Type Header
                 */

                if (jsonInputFormatter != null)
                {
                    jsonInputFormatter.SupportedMediaTypes
                    .Add("application/vnd.iron.tourforcreation+json");

                    jsonInputFormatter.SupportedMediaTypes
                    .Add("application/vnd.iron.tourwithmanagerforcreation+json");

                    jsonInputFormatter.SupportedMediaTypes
                    .Add("application/vnd.iron.tourwithshowsforcreation+json");

                    jsonInputFormatter.SupportedMediaTypes
                    .Add("application/vnd.iron.tourwithmanagerandshowsforcreation+json");

                    jsonInputFormatter.SupportedMediaTypes
                    .Add("application/vnd.iron.showcollectionforcreation+json");

                }

            })
            .AddJsonOptions(options =>
            {
                options.SerializerSettings.DateParseHandling = DateParseHandling.DateTimeOffset;
                options.SerializerSettings.ContractResolver =
                    new CamelCasePropertyNamesContractResolver();
            });

            // Configure CORS so the API allows requests from JavaScript.  
            // For demo purposes, all origins/headers/methods are allowed.  
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOriginsHeadersAndMethods",
                    builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
            });

            // register the DbContext on the container, getting the connection string from
            // appsettings (note: use this during development; in a production environment,
            // it's better to store the connection string in an environment variable)
            var connectionString = Configuration["ConnectionStrings:TourManagementDB"];
            services.AddDbContext<TourManagementContext>(o => o.UseSqlServer(connectionString));

            // register the repository
            services.AddScoped<ITourManagementRepository, TourManagementRepository>();

            // register an IHttpContextAccessor so we can access the current
            // HttpContext in services by injecting it
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // register the user info service
            services.AddScoped<IUserInfoService, UserInfoService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(appBuilder =>
                {
                    appBuilder.Run(async context =>
                    {
                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsync("An unexpected fault happened. Try again later.");
                    });
                });
            }

            AutoMapper.Mapper.Initialize(config =>
            {
                config.CreateMap<Entities.Tour, Dtos.Tour>()
                    .ForMember(d => d.Band, o => o.MapFrom(s => s.Band.Name));

                config.CreateMap<Entities.Tour, Dtos.TourWithEstimatedProfits>()
                   .ForMember(d => d.Band, o => o.MapFrom(s => s.Band.Name));

                config.CreateMap<Entities.Band, Dtos.Band>();
                config.CreateMap<Entities.Manager, Dtos.Manager>();
                config.CreateMap<Entities.Show, Dtos.Show>();

                config.CreateMap<Dtos.TourForCreation, Entities.Tour>();
                config.CreateMap<Dtos.TourWithManagerForCreation, Entities.Tour>();

                config.CreateMap<Entities.Tour, Dtos.TourWithShows>()
                    .ForMember(d => d.Band, o => o.MapFrom(s => s.Band.Name));

                config.CreateMap<Entities.Tour, Dtos.TourWithEstimatedProfitsAndShows>()
                   .ForMember(d => d.Band, o => o.MapFrom(s => s.Band.Name));

                config.CreateMap<Dtos.TourWithShowsForCreation, Entities.Tour>();
                config.CreateMap<Dtos.TourWithManagerAndShowsForCreation, Entities.Tour>();
                config.CreateMap<Dtos.ShowForCreation, Entities.Show>();
            });

            // Enable CORS
            app.UseCors("AllowAllOriginsHeadersAndMethods");
            app.UseMvc();
        }
    }
}
