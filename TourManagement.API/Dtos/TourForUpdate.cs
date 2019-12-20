using System.ComponentModel.DataAnnotations;

namespace TourManagement.API.Dtos
{
    public class TourForUpdate: TourAbstractBase
    {
        //TODO: In update operation Description property is required. However, in the Create operation is not required.
        [Required(AllowEmptyStrings = false,
           ErrorMessage = "required|When updating a tour, the description is required.")]
        public override string Description { get => base.Description; set => base.Description = value; }
    }
}
