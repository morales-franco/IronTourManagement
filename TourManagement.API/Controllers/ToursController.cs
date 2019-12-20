using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using TourManagement.API.Dtos;
using TourManagement.API.Helpers;
using TourManagement.API.Services;

namespace TourManagement.API.Controllers
{
    [Route("api/tours")]
    public class ToursController : Controller
    {
        private readonly ITourManagementRepository _tourManagementRepository;

        public ToursController(ITourManagementRepository tourManagementRepository)
        {
            _tourManagementRepository = tourManagementRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetTours()
        {
            var toursFromRepo = await _tourManagementRepository.GetTours();

            var tours = Mapper.Map<IEnumerable<Tour>>(toursFromRepo);
            return Ok(tours);
        }


        //TODO: [FromHeader(Name = "Accept")] - In this case we don't use the generic filter: RequestHeaderMatchesMediaType 
        // We need to receive acceptHeaderValue and then use the if statement to return a Tour Or TourWithEstimatedProfits
        //[HttpGet("{tourId}")]
        //public async Task<IActionResult> GetTour(Guid tourId,
        //    [FromHeader(Name = "Accept")] string acceptHeaderValue)
        //{
        //    return await GetSpecificTour<Tour>(tourId);
        //} 

        /*
         * TODO: Default GET - When a client call api/tours/123 with Accept: application/json this method will be called.
         * Another alternative could be to decorate GetTour method with:
         * [RequestHeaderMatchesMediaType("accept",
            new string[] { "application/json", "application/vnd.iron.tour+json" })]
         * But it is not a good practice because we want to fit the deal between the API and Client and if we use application/json it wouldn't be the case.
         */
        [HttpGet("{tourId}")]
        public async Task<IActionResult> GetDefaultTour(Guid tourId)
        {
            if(Request.Headers.TryGetValue("Accept",
                out StringValues values))
            {
                Debug.WriteLine($"Accept header(s): { string.Join(",", values) }");
            }

            return await GetSpecificTour<Tour>(tourId);
        }

        [HttpGet("{tourId}", Name = "GetTour")]
        [RequestHeaderMatchesMediaType("accept",
            new string[] { "application/vnd.iron.tour+json" })]
        public async Task<IActionResult> GetTour(Guid tourId)
        {
            return await GetSpecificTour<Tour>(tourId);
        }

        [HttpGet("{tourId}")]
        [RequestHeaderMatchesMediaType("accept",
            new string[] { "application/vnd.iron.tourwithestimatedprofits+json" })]
        public async Task<IActionResult> GetTourWithEstimatedProfits(Guid tourId)
        {
            return await GetSpecificTour<TourWithEstimatedProfits>(tourId);
        }

        [HttpGet("{tourId}")]
        [RequestHeaderMatchesMediaType("accept",
            new string[] { "application/vnd.iron.tourwithshows+json" })]
        public async Task<IActionResult> GetTourWithShows(Guid tourId)
        {
            return await GetSpecificTour<TourWithShows>(tourId, true);
        }

        [HttpGet("{tourId}")]
        [RequestHeaderMatchesMediaType("accept",
            new string[] { "application/vnd.iron.tourwithestimatedprofitsandshows+json" })]
        public async Task<IActionResult> GetTourWithEstimatedProfitsAndShows(Guid tourId)
        {
            return await GetSpecificTour<TourWithEstimatedProfitsAndShows>(tourId, true);
        }

        private async Task<IActionResult> GetSpecificTour<T>(Guid tourId,
            bool includeShows = false) where T : class
        {
            var tourFromRepo = await _tourManagementRepository.GetTour(tourId, includeShows);

            if (tourFromRepo == null)
            {
                return BadRequest();
            }

            return Ok(Mapper.Map<T>(tourFromRepo));
        }

        [HttpPost]
        [RequestHeaderMatchesMediaType("Content-Type",
            new[] { "application/json", "application/vnd.iron.tourforcreation+json" })]
        public async Task<IActionResult> AddTour([FromBody] TourForCreation tour)
        {
            return await AddSpecificTour(tour);
        }

        [HttpPost]
        [RequestHeaderMatchesMediaType("Content-Type",
            new[] { "application/vnd.iron.tourwithmanagerforcreation+json" })]
        public async Task<IActionResult> AddTour([FromBody] TourWithManagerForCreation tour)
        {
            return await AddSpecificTour(tour);
        }

        public async Task<IActionResult> AddSpecificTour<T>(T tour) where T : class
        {
            if (tour == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return new UnprocessableEntityObjectResult(ModelState);
            }

            var tourEntity = Mapper.Map<Entities.Tour>(tour);

            if (tourEntity.ManagerId == Guid.Empty)
            {
                tourEntity.ManagerId = new Guid("fec0a4d6-5830-4eb8-8024-272bd5d6d2bb");
            }

            await _tourManagementRepository.AddTour(tourEntity);

            if (!await _tourManagementRepository.SaveAsync())
            {
                throw new Exception("Adding a tour failed on save");
            }

            var tourToReturn = Mapper.Map<Tour>(tourEntity);

            return CreatedAtRoute("GetTour",
                new { tourId = tourToReturn.TourId },
                tourToReturn);

        }

        [HttpPost]
        [RequestHeaderMatchesMediaType("Content-Type",
           new[] { "application/json", "application/vnd.iron.tourwithshowsforcreation+json" })]
        public async Task<IActionResult> AddTourWithShows([FromBody] TourWithShowsForCreation tour)
        {
            return await AddSpecificTour(tour);
        }

        [HttpPost]
        [RequestHeaderMatchesMediaType("Content-Type",
           new[] { "application/json", "application/vnd.iron.tourwithmanagerandshowsforcreation+json" })]
        public async Task<IActionResult> AddTourWithManagerAndShows(
            [FromBody] TourWithManagerAndShowsForCreation tour)
        {
            return await AddSpecificTour(tour);
        }

        //PATH is great and powerfull however it could be complex if we want to update more than one level deep object.
        [HttpPatch("{tourId}")]
        public async Task<IActionResult> PartiallyUpdateTour(Guid tourId,
          [FromBody] JsonPatchDocument<TourForUpdate> jsonPatchDocument)
        {
            if (jsonPatchDocument == null)
            {
                return BadRequest();
            }

            var tourFromRepo = await _tourManagementRepository.GetTour(tourId);

            if (tourFromRepo == null)
            {
                return BadRequest();
            }

            var tourToPatch = Mapper.Map<TourForUpdate>(tourFromRepo);

            //TODO: jsonPatchDocument was created using an External Angular Library, if this library create
            // a bad patch document (bad structure) the model state contains an error in this step.
            jsonPatchDocument.ApplyTo(tourToPatch, ModelState);

            if (!ModelState.IsValid)
            {
                return new UnprocessableEntityObjectResult(ModelState);
            }

            //TODO: If the json patch structure is ok, we need to check that the new model is valid!
            if (!TryValidateModel(tourToPatch))
            {
                return new UnprocessableEntityObjectResult(ModelState);
            }


            Mapper.Map(tourToPatch, tourFromRepo);

            await _tourManagementRepository.UpdateTour(tourFromRepo);

            if (!await _tourManagementRepository.SaveAsync())
            {
                throw new Exception("Updating a tour failed on save.");
            }

            return NoContent();
        }



    }
}
