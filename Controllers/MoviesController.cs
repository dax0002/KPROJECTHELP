using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FinalProject.DAL;
using FinalProject.Models;

namespace FinalProject.Controllers
{
    public class MoviesController : Controller
    {
        private readonly AppDbContext _context;

        public MoviesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Movies
        public IActionResult Index(string SearchString)
        {
            var query = from m in _context.Movies
                        select m;

            if (SearchString != null)
            {
                query = query.Where(m => m.MovieTitle.Contains(SearchString) ||
                                m.Tagline.Contains(SearchString) ||
                                Convert.ToString(m.ReleaseYear).Contains(SearchString) ||
                                m.Genre.GenreTitle.Contains(SearchString) ||
                                m.Rating.MPAARating.Contains(SearchString) ||
                                Convert.ToString(m.Reviews).Contains(SearchString) ||
                                m.Actors.Contains(SearchString));
            }

            List<Movie> SelectedMovies = query.Include(m => m.Genre)
                                              .Include(m => m.Reviews)
                                              .Include(m => m.Rating)
                                              .Include(m => m.Schedules).ToList();


            // Populate the view bag with a count of all job postings
            ViewBag.AllMovies = _context.Movies.Count();
            //Populate the view bag with a count of selected job postings
            ViewBag.SelectedMovies = SelectedMovies.Count();

            return View(SelectedMovies.OrderByDescending(m => m.ReleaseYear));
        }

        // GET: Movies/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null) //JobPostingID not specified
            {
                return View("Error", new String[] { "MovieID not specified - which movie do you want to view?" });
            }

            Movie movie = _context.Movies
                                .Include(m => m.Genre)
                                .Include(m => m.Rating)
                                .Include(m => m.Reviews)
                                .FirstOrDefault(m => m.MovieID == id);

            if (movie == null) //Job posting does not exist in database
            {
                return View("Error", new String[] { "Movie not found in database" });
            }
            //if code gets this far, all is well
            return View(movie);
        }

        // WE NEED CREATE FOR MANAGERS
        /*
        // GET: Movies/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Movies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MovieID,MovieTitle,Description,Tagline,ReleaseYear,Runtime,Actors")] Movie movie)
        {
            if (ModelState.IsValid)
            {
                _context.Add(movie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(movie);
        }
        */

        // DO WE NEED EDIT???
        /*
        // GET: Movies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Movies == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }
            return View(movie);
        }

        // POST: Movies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MovieID,MovieTitle,Description,Tagline,ReleaseYear,Runtime,Actors")] Movie movie)
        {
            if (id != movie.MovieID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(movie);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieExists(movie.MovieID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(movie);
        }
        */

        // I DONT THINK WE NEED THIS
        /*
        // GET: Movies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Movies == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies
                .FirstOrDefaultAsync(m => m.MovieID == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Movies == null)
            {
                return Problem("Entity set 'AppDbContext.Movies'  is null.");
            }
            var movie = await _context.Movies.FindAsync(id);
            if (movie != null)
            {
                _context.Movies.Remove(movie);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        */

        private bool MovieExists(int id)
        {
            return _context.Movies.Any(e => e.MovieID == id);
        }

        public IActionResult DetailedSearch(String SearchString)
        {
            ViewBag.AllGenres = GetAllGenresSelectList();

            SearchViewModel svm = new SearchViewModel();

            return View(svm);
        }

        private SelectList GetAllGenresSelectList()
        {
            List<Genre> genreList = _context.Genres.ToList();

            Genre SelectNone = new Genre() { GenreID = 0, GenreTitle = "All Genres" };
            genreList.Add(SelectNone);

            SelectList genresSelectList = new SelectList(genreList.OrderBy(m => m.GenreTitle), "GenreID", "GenreTitle");

            return genresSelectList;
        }

        
        public IActionResult DisplaySearchResults(SearchViewModel svm)
        {
            var query = from c in _context.Movies
                        select c;

            if (svm.SearchTitle != null)
            {
                query = query.Where(c => c.MovieTitle.Contains(svm.SearchTitle));
            }

            if (svm.SearchTagline != null)
            {
                query = query.Where(c => c.Tagline.Contains(svm.SearchTagline));
            }

            if (svm.SelectedGenre != 0)
            {
                query = query.Where(c => c.Genre.GenreID == svm.SelectedGenre);
            }

            if (svm.SearchReleaseYear != 0)
            {
                query = query.Where(c => c.ReleaseYear == svm.SearchReleaseYear);
            }

            /*
            //REVIEWS???????? 
            if (svm.SearchReview != null)
            {
                if (svm.SearchType == SearchType.GreaterThan)
                {
                    query = query.Where(c => c.Reviews >= Decimal.Parse(svm.SearchReview));
                }

                if (svm.SearchType == SearchType.LessThan)
                {
                    query = query.Where(c => c.Reviews <= Decimal.Parse(svm.SearchReview));
                }
            }
            */

            if (svm.SearchRating != null)
            {
                query = query.Where(c => c.Rating.MPAARating == svm.SearchRating);
            }

            if (svm.SelectedActors != null)
            {
                query = query.Where(c => c.Actors.Contains(svm.SelectedActors));
            }

            List<Movie> SelectedMovies = query.Include(c => c.Genre)
                                              .Include(c => c.Reviews)
                                              .Include(c => c.Rating)
                                              .Include(c => c.Schedules)
                                              .ToList();

            /*
            IEnumerable<Movie> SelectedMovies = query.Include(c => c.Genre)
                                              .Include(c => c.Reviews)
                                              .Include(c => c.Rating)
                                              .Include(c => c.Schedules)
                                              .Include(c => c.Actors)
                                              .ToList()
                                              .Select(c => new Movie
                                              {
                                                  MovieID = c.MovieID,
                                                  MovieTitle = c.MovieTitle,
                                                  Description = c.Description,
                                                  Tagline = c.Tagline,
                                                  ReleaseYear = c.ReleaseYear,
                                                  Runtime = c.Runtime,
                                                  Actors = c.Actors,
                                                  Genre = c.Genre,
                                                  Rating = c.Rating,
                                          
                                              })
                                              .OrderByDescending(c => c.ReleaseYear);
                                                  //AvgRating = c.Reviews.Average(od => od.Rating); // Assuming Reviews is a collection of some type
                                                                                                  // Include other properties as needed; 
            */

            //List<JobPosting> SelectedJobPostings = query.ToList();

            // Populate the view bag with a count of all job postings
            ViewBag.AllMovies = _context.Movies.Count();
            //Populate the view bag with a count of selected job postings
            ViewBag.SelectedMovies = SelectedMovies.Count();

            return View("Index", SelectedMovies.OrderByDescending(c => c.ReleaseYear));
        }
        
    }
}
