﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TourManagement.API.Dtos;
using TourManagement.API.Helpers;
using TourManagement.API.Services;

namespace TourManagement.API.Controllers
{
    [Produces("application/json")]
    [Route("api/tours/{tourId}/showcollections")]
    public class ShowCollectionsController : Controller
    {
        private readonly ITourManagementRepository _tourManagementRepository;

        public ShowCollectionsController(ITourManagementRepository tourManagementRepository)
        {
            _tourManagementRepository = tourManagementRepository;
        }

        //TODO: URL that receives IEnumerable<> in the querystring api/tours/{tourId}/showcollections/(id1,id2, … )
        [HttpGet("({showIds})", Name = "GetShowCollection")]
        [RequestHeaderMatchesMediaType("Accept", new[] { "application/json",
            "application/vnd.iron.showcollection+json" })]
        public async Task<IActionResult> GetShowCollection(Guid tourId,
          [ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<Guid> showIds)
        {
            if (showIds == null || !showIds.Any())
            {
                return BadRequest();
            }

            // check if the tour exists
            if (!await _tourManagementRepository.TourExists(tourId))
            {
                return NotFound();
            }

            var showEntities = await _tourManagementRepository.GetShows(tourId, showIds);

            if (showIds.Count() != showEntities.Count())
            {
                return NotFound();
            }

            var showCollectionToReturn = Mapper.Map<IEnumerable<Show>>(showEntities);
            return Ok(showCollectionToReturn);
        }

        [HttpPost]
        [RequestHeaderMatchesMediaType("Content-Type",
           new[] { "application/json", "application/vnd.iron.showcollectionforcreation+json" })]
        public async Task<IActionResult> CreateShowCollection(
            Guid tourId,
            [FromBody] IEnumerable<ShowForCreation> showCollection)
        {
            if(showCollection == null)
            {
                return BadRequest();
            }

            if(!await _tourManagementRepository.TourExists(tourId))
            {
                return NotFound();
            }

            var showEntities = Mapper.Map<IEnumerable<Entities.Show>>(showCollection);

            foreach (var show in showEntities)
            {
                await _tourManagementRepository.AddShow(tourId, show);
            }

            if(!await _tourManagementRepository.SaveAsync())
            {
                throw new Exception("Adding a collection of shows failed on save");
            }

            var showCollectionToReturn = Mapper.Map<IEnumerable<Show>>(showEntities);

            var showIdsAsString = string.Join(",", showCollectionToReturn.Select(a => a.ShowId));

            return CreatedAtRoute("GetShowCollection",
              new { tourId, showIds = showIdsAsString },
              showCollectionToReturn);

        }

    }
}