using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CoreCodeCamp.Data;
using CoreCodeCamp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace CoreCodeCamp.Controllers
{

    [Route("api/[controller]")]
    [ApiVersion("1.0")]
    [ApiVersion("1.1")]
    [ApiController]
    public class CampsController : ControllerBase
    {
        private readonly ICampRepository _campRepository;
        private readonly IMapper _mapper;
        private readonly LinkGenerator _linkGenerator;

        public CampsController(ICampRepository campRepository, IMapper mapper, LinkGenerator linkGenerator)
        {
            _campRepository = campRepository;
            _mapper = mapper;
            _linkGenerator = linkGenerator;
        }

        [HttpGet]
        public async Task<ActionResult<CampModel[]>> Get(bool includeTalks = false)
        {
            var campModelList = new List<CampModel>();
            try
            {
                var results = await _campRepository.GetAllCampsAsync(includeTalks);

                foreach (var result in results)
                {
                    campModelList.Add(ConvertCampToCampModel(result));
                }

                return campModelList.ToArray();
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpGet("{moniker}")]
        [MapToApiVersion("1.0")]
        public async Task<ActionResult<CampModel>> Get(string moniker, bool includeTalks = false)
        {
            try
            {
                var result = await _campRepository.GetCampAsync(moniker, includeTalks);

                if (result == null) return NotFound();

                return ConvertCampToCampModel(result);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpGet("{moniker}")]
        [MapToApiVersion("1.1")]
        public async Task<ActionResult<CampModel>> Get11(string moniker, bool includeTalks = false)
        {
            try
            {
                var result = await _campRepository.GetCampAsync(moniker, includeTalks);

                if (result == null) return NotFound();

                return ConvertCampToCampModel(result);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<CampModel[]>> SearchByDate(DateTime searchDate, bool includeTalks = false)
        {
            var campModelList = new List<CampModel>();
            try
            {
                var results = await _campRepository.GetAllCampsByEventDate(searchDate, includeTalks);

                if (!results.Any()) return NotFound();

                foreach (var result in results)
                {
                    campModelList.Add(ConvertCampToCampModel(result));
                }

                return campModelList.ToArray();

            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpPost]
        public async Task<ActionResult<CampModel>> Post(CampModel model)
        {
            try
            {
                var existingCamp = await _campRepository.GetCampAsync(model.Moniker);

                if (existingCamp != null)
                {
                    return BadRequest("Moniker In Use");
                }

                var location = _linkGenerator.GetPathByAction("Get", "Camps", new { moniker = model.Moniker });

                if (string.IsNullOrWhiteSpace(location))
                {
                    return BadRequest("Could not use moniker");
                }

                var camp = _mapper.Map<Camp>(model);

                _campRepository.Add(camp);

                if (await _campRepository.SaveChangesAsync())
                {
                    return Created(location, ConvertCampToCampModel(camp));
                }
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }

            return BadRequest();
        }

        [HttpPut("{moniker}")]
        public async Task<ActionResult<CampModel>> Put(string moniker, CampModel model)
        {
            try
            {
                var oldCamp = await _campRepository.GetCampAsync(moniker);
                if (oldCamp == null) return NotFound("Could not find camp");

                _mapper.Map(model, oldCamp);

                if (await _campRepository.SaveChangesAsync())
                {
                    return ConvertCampToCampModel(oldCamp);
                }
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }

            return BadRequest();

        }

        [HttpDelete("moniker")]
        public async Task<IActionResult> Delete(string moniker)
        {
            try
            {
                var oldCamp = await _campRepository.GetCampAsync(moniker);

                if (oldCamp == null)
                {
                    return NotFound("Could not find camp.");
                }

                _campRepository.Delete(oldCamp);

                if (await _campRepository.SaveChangesAsync())
                {
                    return Ok();
                }

            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }

            return BadRequest();
        }

        private CampModel ConvertCampToCampModel(Camp camp)
        {
            var campModel = new CampModel
            {
                EventDate = camp.EventDate,
                Length = camp.Length,
                LocationAddress1 = camp.Location.Address1,
                LocationAddress2 = camp.Location.Address2,
                LocationAddress3 = camp.Location.Address3,
                LocationCityTown = camp.Location.CityTown,
                LocationCountry = camp.Location.Country,
                LocationPostalCode = camp.Location.PostalCode,
                LocationStateProvince = camp.Location.StateProvince,
                Moniker = camp.Moniker,
                Venue = camp.Location.VenueName,
                Name = camp.Name,
                Talks = new List<TalkModel>()

            };

            foreach (var talk in camp.Talks)
            {
                campModel.Talks.Add(new TalkModel
                {
                    Title = talk.Title,
                    Abstract = talk.Abstract,
                    Level = talk.Level,
                    Speaker = new SpeakerModel
                    {
                        FirstName = talk.Speaker.FirstName,
                        LastName = talk.Speaker.LastName,
                        BlogUrl = talk.Speaker.BlogUrl,
                        Company = talk.Speaker.Company,
                        CompanyUrl = talk.Speaker.CompanyUrl,
                        GitHub = talk.Speaker.GitHub,
                        MiddleName = talk.Speaker.MiddleName,
                        Twitter = talk.Speaker.Twitter
                    }
                });
            }

            return campModel;
        }
    }
}
