using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using HW60_Stackoverflow_May2.Models;
using ClassLibrary1;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using System.IO;
using Microsoft.AspNetCore.Authorization;




namespace HW60_Stackoverflow_May2.Controllers
{
    public class HomeController : Controller
    {
        private string _connectionString;
        private IHostingEnvironment _environment;

        public HomeController(IConfiguration configuration,
             IHostingEnvironment environment)
        {
            _environment = environment;
            _connectionString = configuration.GetConnectionString("ConStr");
        }

        public IActionResult Index()
        {
            var mgr = new StackOverflowManager(_connectionString);
            IEnumerable<Question> questions = mgr.GetQuestions();
            return View(questions);
        }
        public IActionResult ViewQuestion(int id)
        {
            var mgr = new StackOverflowManager(_connectionString);
            ViewQuestionViewModel vm = new ViewQuestionViewModel();
            vm.Question = mgr.GetQuestion(id);
            vm.Likes = mgr.GetCountLikesForQuestion(id);
            vm.User =  mgr.GetByEmail(User.Identity.Name);
           if(vm.User == null)
            {
                vm.CanLikeQuestion = false;
            }
            else
            {
                vm.CanLikeQuestion = mgr.AlreadyLiked(id, vm.User.Id);
            }
            vm.Tags = mgr.GetTags(id);
            vm.Answers = mgr.GetAnswers(id);
            return View(vm);
        }
        private bool HasPermissionToView(int id)
        {
            List<int> allowedIds = HttpContext.Session.Get<List<int>>("allowedids");
            if (allowedIds == null)
            {
                return false;
            }
            HttpContext.Session.Set("allowedids", allowedIds);
            return allowedIds.Contains(id);
        }
        [HttpPost]
        public IActionResult SubmitAnswer(string text, int questionid, int userid)
        {
            var mgr = new StackOverflowManager(_connectionString);
            mgr.AddAnswer(text, questionid, userid);
            return RedirectToAction("viewquestion", new { id = questionid });
        }
       [Authorize]  
        public IActionResult AskAQuestion()
        {

            return View();
        }
        [HttpPost]
        public IActionResult SubmitQuestion(Question question, IEnumerable<string> tags)
        {
            question.DatePosted = DateTime.Now;
            var mgr = new StackOverflowManager(_connectionString);
            mgr.AddQuestion(question, tags);
            return Redirect("/");
        }
       public IActionResult LikeQuestion (int questionId)
        {
            var mgr = new StackOverflowManager(_connectionString);
            User user = mgr.GetByEmail(User.Identity.Name);
            mgr.AddLike(questionId, user.Id);
            return RedirectToAction("viewquestion", new { id = questionId });
        }
    }
    public static class SessionExtensions
    {
        public static void Set<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        public static T Get<T>(this ISession session, string key)
        {
            string value = session.GetString(key);

            return value == null ? default(T) :
                JsonConvert.DeserializeObject<T>(value);
        }
    }
}

//For this homework, we're going to create a simple StackOverflow clone. 
//    Here's the functionality it should support:

//On the home page, the user should see a list of all the questions sorted by most recent first.
//    (As a bonus, next to each question, display the amount of likes, as well as the tags associated with that question).
//The title of the question should be a link that takes you to the question page.

//On the question page, display the full text of the question, as well as the tags and likes for that question. 
//    Beneath that, display all the answers for that question, (as a bonus, also display the email of the users for each answer). 
//    If the user is currently logged in, they should also see a Like button next to the question to allow them to like it. 
//    Once a user has liked a question, they should not be able to like it again. 
    
//    Also, logged in users should have a textbox underneath the question where they can post their own answer.

//In the navbar, there should be a link that says "Ask a question".
//    If the current user is not logged in, they should get redirected to a login page.
//    If they are logged in, they should be taken to a page where they can ask a question
//    (title, text and tags -use the code I wrote in class as a guide).


//You'll obviously also need a signup page where new users can sign up.

//As another bonus, add likes to Answers as well.
//    Again, only logged in users should be able to like answers, and see if you can display the likes next to each answer as well.
//    To add this feature of liking Answers, you'll need another many to many table (you can call it LikedAnswers)
//        that will be a many to many from Users to Answers.


//I know I showed some instances where you can make it more performant, 
//        but for this exercise I would advise not to worry about performance.
//        Get it all working first, that's the main goal.


//Good luck!!