using AutoMapper;
using CoreCodeCamp.Controllers;
using CoreCodeCamp.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc.Versioning.Conventions;
using Microsoft.Extensions.DependencyInjection;

namespace CoreCodeCamp
{
  public class Startup
  {
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddDbContext<CampContext>();
      services.AddScoped<ICampRepository, CampRepository>();

      services.AddApiVersioning(options =>
      {
          options.AssumeDefaultVersionWhenUnspecified = true;
          options.DefaultApiVersion = new ApiVersion(1, 1);
          options.ReportApiVersions = true;
          //options.ApiVersionReader = new QueryStringApiVersionReader("version"); //default is api-version

          options.ApiVersionReader = ApiVersionReader.Combine(new HeaderApiVersionReader("x-version"), 
              new QueryStringApiVersionReader("version"));

          options.Conventions.Controller<TalksController>().HasApiVersion(new ApiVersion(1, 0))
              .HasApiVersion(new ApiVersion(1, 1))
              .Action(x => x.Delete(default(string), default(int)))
              .MapToApiVersion(1, 1);
      });

      services.AddAutoMapper(typeof(Startup));

      services.AddMvc(options => options.EnableEndpointRouting = false)
        .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
    }

    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }
      
      app.UseMvc();
    }
  }
}
