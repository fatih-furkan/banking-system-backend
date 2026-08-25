using Bank.CampaignService.Data;
using Bank.CampaignService.Models.Entities;
using Bank.Shared.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace Bank.CampaignService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    
    public class CampaignsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CampaignsController(AppDbContext context)
        {
            _context = context;
        }

        ///------------------------------------------------------------------
        /// Retrieves campaigns with optional status and target date filters.
        /// Includes criteria list for each campaign
        /// -----------------------------------------------------------------
       

        [HttpGet]
        public async Task<IActionResult> GetCampaigns(
            [FromQuery] CampaignStatus? status,
            [FromQuery] DateTime? targetDate)
        {
            var query = _context.Campaigns
                .Include(c => c.Criteria)
                .AsNoTracking()
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(c => c.Status == status.Value);
            }
            
            if (targetDate.HasValue)
            {
                var dateOnly = targetDate.Value.Date;
                query = query.Where(c => c.StartDate.Date <= dateOnly && c.EndDate.Date >= dateOnly);
            }

            var campaigns = await query.ToListAsync();

            return Ok(new
            {
                success = true,
                message = "Campaigns retrieved successfully.",
                data = campaigns
            });
        }
        ///-----------------------------------------------------------------
        /// Retrieves a single campaign by ID along with its criteria list. 
        ///-----------------------------------------------------------------


        [HttpGet("{id}")]
        public async Task<IActionResult> GetCampaignById(long id)
        {
            var campaign = await _context.Campaigns
                .Include(c => c.Criteria)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CampaignId == id);

            if (campaign == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = $"Campaign with ID {id} not found."
                });
            }

            return Ok(new
            {
                success = true,
                data = campaign
            });
        }

        ///------------------------------------------------
        /// Creates a new campaign with its criteria list.
        ///------------------------------------------------

        [HttpPost]
        public async Task<IActionResult> CreateCampaign([FromBody] Campaign campaign)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, errors = ModelState });
            }

            campaign.StartDate = campaign.StartDate.Date;
            campaign.EndDate = campaign.EndDate.Date;


            if (campaign.Criteria != null && campaign.Criteria.Any())
            {
                foreach (var criterion in campaign.Criteria)
                {
                    if (criterion.TransactionStartDate.HasValue)
                        criterion.TransactionStartDate = criterion.TransactionStartDate.Value.Date;

                    if (criterion.TransactionEndDate.HasValue)
                        criterion.TransactionEndDate = criterion.TransactionEndDate.Value.Date;
                }
            }

            await _context.Campaigns.AddAsync(campaign);
            await _context.SaveChangesAsync();

            return StatusCode(201, new
            {
                success = true,
                message = "Campaign created successfully.",
                data = campaign
            });
        }

        ///----------------------------------------------------
        /// Updates an existing campaign and its criteria list 
        ///----------------------------------------------------

        [HttpPut("{id}")]

        public async Task<IActionResult> UpdateCampaign(long id, [FromBody] Campaign updatedCampaign)
        {
            if (id != updatedCampaign.CampaignId)
            {
                return BadRequest(new { success = false, message = "ID mismatch." });
            }

            var existingCampaign = await _context.Campaigns
                .Include(c => c.Criteria)
                .FirstOrDefaultAsync(c => c.CampaignId == id);

            if (existingCampaign == null)
            {
                return NotFound(new { success = false, message = $"Campaign with ID {id} not found." });
            }


            existingCampaign.Name = updatedCampaign.Name;
            existingCampaign.Description = updatedCampaign.Description;
            existingCampaign.StartDate = updatedCampaign.StartDate.Date;
            existingCampaign.EndDate = updatedCampaign.EndDate.Date;
            existingCampaign.RewardType = updatedCampaign.RewardType;
            existingCampaign.CalculationCriteria = updatedCampaign.CalculationCriteria;
            //  existingCampaign.TotalBudget = updatedCampaign.TotalBudget;
            // existingCampaign.UsedBudget = updatedCampaign.UsedBudget;
            existingCampaign.DailyLimit = updatedCampaign.DailyLimit;
            existingCampaign.Status = updatedCampaign.Status;

            if (updatedCampaign.Criteria != null)
            {
                _context.CampaignCriteria.RemoveRange(existingCampaign.Criteria);

                foreach (var criterion in updatedCampaign.Criteria)
                {
                    
                    if (criterion.TransactionStartDate.HasValue)
                        criterion.TransactionStartDate = criterion.TransactionStartDate.Value.Date;

                    if (criterion.TransactionEndDate.HasValue)
                        criterion.TransactionEndDate = criterion.TransactionEndDate.Value.Date;

                    criterion.CriterionId = 0; 
                    existingCampaign.Criteria.Add(criterion);
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Campaign updated successfully.",
                data = existingCampaign
            });
        }

        ///------------------------------------------------------------------
        /// Deletes a campaign and its associated criteria from the database.
        ///------------------------------------------------------------------
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCampaign(long id)
        {
            var campaign = await _context.Campaigns
                .Include(c => c.Criteria)
                .FirstOrDefaultAsync(c => c.CampaignId == id);

            if (campaign == null)
            {
                return NotFound(new { success = false, message = $"Campaign with ID {id} not found." });
            }

            _context.Campaigns.Remove(campaign);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = $"Campaign with ID {id} deleted successfully."
            });
        }
    }
}
