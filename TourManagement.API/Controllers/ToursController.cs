using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
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


        [HttpGet("{tourId}")]
        [RequestHeaderMatchesMediaType("accept", 
            new string[] { "application/vnd.iron.tour+json" })]
        public async Task<IActionResult> GetTour(Guid tourId)
        {
            var tourFromRepo = await _tourManagementRepository.GetTour(tourId);

            if (tourFromRepo == null)
            {
                return BadRequest();
            }

            var tour = Mapper.Map<Tour>(tourFromRepo);

            return Ok(tour);
        }

        [HttpGet("{tourId}")]
        [RequestHeaderMatchesMediaType("accept",
            new string[] { "application/vnd.iron.tourwithestimatedprofits+json" })]
        public async Task<IActionResult> GetTourWithEstimatedProfits(Guid tourId)
        {
            var tourFromRepo = await _tourManagementRepository.GetTour(tourId);

            if (tourFromRepo == null)
            {
                return BadRequest();
            }

            var tour = Mapper.Map<Tour>(tourFromRepo);

            return Ok(tour);
        }

        

    }
}
