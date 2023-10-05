using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using League.Models;
using League.Data;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace League.Pages.Players
{
    public class IndexModel : PageModel
    {
        // inject the Entity Framework context

        private readonly LeagueContext _context;

        public IndexModel(LeagueContext context)
        {
            _context = context;
        }

        public List<Player> Players { get; set; }

        // make a series of variables for the filter and search form

        [BindProperty(SupportsGet = true)]
        public string SearchString { get; set; }

        public SelectList Teams { get; set; }
        public SelectList Positions { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SelectedTeam { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SelectedPosition { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SortField { get; set; } = "Name";

        // read the favorite team cookie into this member variable

        public string FavouriteTeam { get; set; }

        
        public int CurrentPage { get; set; }

        public int LastPage { get; set; }

        public async Task OnGetAsync(int PageNumber)
        {
            // create a base query that retrieves all players

            var players = from p in _context.Players
                          select p;

            // modify the query is the user is searching

            if (!string.IsNullOrEmpty(SearchString))
            {
                players = players.Where(p => p.Name.Contains(SearchString));
            }

            // modify the query is the user is filtering

            if (!string.IsNullOrEmpty(SelectedTeam))
            {
                players = players.Where(p => p.TeamId == SelectedTeam);
            }

            if (!string.IsNullOrEmpty(SelectedPosition))
            {
                players = players.Where(p => p.Position == SelectedPosition);
            }

            // modify the query is the user is sorting

            switch (SortField)
            {
                case "Number": players = players.OrderBy(p => p.Number).ThenBy(p => p.TeamId); break;
                case "Name": players = players.OrderBy(p => p.Name).ThenBy(p => p.TeamId); break;
                case "Position": players = players.OrderBy(p => p.Position).ThenBy(p => p.TeamId); break;
            }

            // make 2 select lists for the filter dropdowns

            IQueryable<string> teamQuery = from t in _context.Teams
                                           orderby t.TeamId
                                           select t.TeamId;

            Teams = new SelectList(await teamQuery.ToListAsync());

            IQueryable<string> positionQuery = from p in _context.Players
                                               orderby p.Position
                                               select p.Position;

            Positions = new SelectList(await positionQuery.Distinct().ToListAsync());

            // read the favorite team from a cookie

            FavouriteTeam = HttpContext.Session.GetString("_Favourite");

            // finally retrieve any players according to filter, search, and sort options

            int NumPlayers = await players.CountAsync();

            int PageSize = 30;

            CurrentPage = PageNumber;

            LastPage = (int)System.Math.Round((double)NumPlayers / PageSize);

            Players = await players
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }

        // return a string for the class of each player <a> tag, bold for starter, gold for favorite

        public string PlayerClass(Player Player)
        {
            string Class = "d-flex";
            if (Player.Depth == 1)
                Class += " starter";
            if (Player.TeamId == FavouriteTeam)
                Class += " favorite";
            return Class;
        }
    }
}