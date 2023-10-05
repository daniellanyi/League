using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using League.Data;
using League.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace League.Pages
{
  public class IndexModel : PageModel
  {
    private readonly LeagueContext _context;
    
    public IndexModel(LeagueContext context)
    {
        _context = context;
    }

    public List<Conference> Conferences { get; set; }
    public List<Division> Divisions { get; set; }
    public List<Team> Teams { get; set; }

    [BindProperty(SupportsGet = true)]
    public string FavouriteTeam { get; set; }

    [BindProperty(SupportsGet = true)]
    public string ViewMode { get; set; }

    public SelectList AllTeams { get; set; }
    public List<Team> AllTeamsByWin { get ; set; }

    public async Task OnGetAsync()
    {
        Conferences =  await _context.Conferences.ToListAsync();
        Divisions = await _context.Divisions.ToListAsync();
        Teams = await _context.Teams.ToListAsync();

        IQueryable<string> teamIDQuery = from t in _context.Teams
                                       orderby t.TeamId
                                       select t.TeamId;
        AllTeams = new SelectList(await teamIDQuery.ToListAsync());

        IQueryable<Team> teamQuery = from t in _context.Teams
                                            orderby t.Win descending
                                            select t;

        AllTeamsByWin = new List<Team>(await teamQuery.ToListAsync());



        if (ViewMode != null)
        {
            HttpContext.Session.SetString("_ViewMode", ViewMode);
        }
        else
        {
            ViewMode = HttpContext.Session.GetString("_ViewMode");
            if (ViewMode == null)
            {
                ViewMode = "Division";
                HttpContext.Session.SetString("_ViewMode", "Division");
            }
        }

        if (FavouriteTeam != null)
        {
            HttpContext.Session.SetString("_Favourite", FavouriteTeam);
        }
        else
        {
            FavouriteTeam = HttpContext.Session.GetString("_Favourite");
        }

        ViewData["PartialViewRoute"] = $"./_{ViewMode}Partial";
    }


    public List<Team> GetTeamsFromConference(string ConferenceId)
    {
        return Teams.Where(team => Divisions.Any(division => division.ConferenceId == ConferenceId && division.DivisionId == team.DivisionId)).OrderByDescending(t => t.Win).ToList();
    }

        public List<Division> GetDivisionsFromConference(string ConferenceId)
    {
        return Divisions.Where(d => d.ConferenceId.Equals(ConferenceId)).OrderBy(d => d.Name).ToList();
    }

    public List<Team> GetTeamsFromDivision(string DivisionId)
    {
        return Teams.Where(t => t.DivisionId.Equals(DivisionId)).OrderByDescending(t => t.Win).ToList();    
    }
  }
}
