using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Data.SqlClient;
using System.Linq;

namespace ClassLibrary1
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public List<Like> Likes { get; set; }
        public List<Answer> Answers { get; set; }
    }
    public class Like
    {
        public int UserId { get; set; }
        public int QuestionId { get; set; }
        public User User { get; set; }
        public Question Question { get; set; }
    }
    public class Question
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public DateTime DatePosted { get; set; }
        public List<QuestionsTags> QuestionsTags { get; set; }
        public List<Like> Likes { get; set; }
        public List<Answer> Answers { get; set; }
    }
    public class QuestionsTags
    {
        public int QuestionId { get; set; }
        public int TagId { get; set; }
        public Question Question { get; set; }
        public Tag Tag { get; set; }
    }
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<QuestionsTags> QuestionsTags { get; set; }
    }
    public class Answer
    {
        public int Id { get; set; }
        public int QuestionId { get; set; }
        public int UserId { get; set; }
        public string Text { get; set; }
        public Question Question { get; set; }
        public User User { get; set; }
    }
    public class StackOverflowManager
    {
        private string _connectionString;

        public StackOverflowManager(string connectionString)
        {
            _connectionString = connectionString;
        }
        #region Question
        public void AddQuestion(Question question, IEnumerable<string> tags)
        {
            using (var ctx = new StackOverflowContext(_connectionString))
            {
                ctx.Questions.Add(question);
                foreach (string tag in tags)
                {
                    Tag t = GetTag(tag);
                    int tagId;
                    if (t == null)
                    {
                        tagId = AddTag(tag);
                    }
                    else
                    {
                        tagId = t.Id;
                    }
                    ctx.QuestionsTags.Add(new QuestionsTags
                    {
                        QuestionId = question.Id,
                        TagId = tagId
                    });
                }

                ctx.SaveChanges();
            }
        }
            public IEnumerable<Question> GetQuestions()
        {
            using (var ctx = new StackOverflowContext(_connectionString))
            {
                return ctx.Questions.ToList().OrderByDescending(o => o.DatePosted);
            }
        }
        public Question GetQuestion(int id)
        {
            using (var ctx = new StackOverflowContext(_connectionString))
            {
                return ctx.Questions.FirstOrDefault(q => q.Id == id);
            }
        }
        #endregion
        #region User
        public User GetUserById(int id)
        {
            using (var ctx = new StackOverflowContext(_connectionString))
            {
                return ctx.Users.FirstOrDefault(t => t.Id == id);
            }
        }
        public User GetByEmail(string email)
        {
            using (var ctx = new StackOverflowContext(_connectionString))
            {
                return ctx.Users.FirstOrDefault(t => t.Email == email);
            }
        }
        public User Login(string email, string password)
        {
            var user = GetByEmail(email);
            if (user == null)
            {
                return null;
            }

            bool isCorrectPassword = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            if (isCorrectPassword)
            {
                return user;
            }

            return null;
        }
        public void AddUser(User user, string password)
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
            using (var ctx = new StackOverflowContext(_connectionString))
            {
                ctx.Users.Add(user);
                ctx.SaveChanges();
            }
        }
        #endregion
        #region Like
        public void AddLike(int questionid, int userid)
        {
            Like like = new Like();
            like.QuestionId = questionid;
            like.UserId = userid;
            using (var ctx = new StackOverflowContext(_connectionString))
            {
                ctx.Likes.Add(like);
                ctx.SaveChanges();
            }

        }
        public IEnumerable<Like> GetLikes()
        {
            using (var ctx = new StackOverflowContext(_connectionString))
            {
                return ctx.Likes.ToList();
            }
        }
        public int GetCountLikesForQuestion(int questionId)
        {
            using (var ctx = new StackOverflowContext(_connectionString))
            {
                return ctx.Likes.Count(l => l.QuestionId == questionId);
            }
        }
        public bool AlreadyLiked(int questionId, int userId)
        {
            IEnumerable<Like> likes = GetLikes();
            foreach(Like like in likes)
            {
                if(like.QuestionId == questionId && like.UserId == userId)
                {
                    return false;
                }
            }
            return true;
        }
        #endregion
        #region Tag
        private Tag GetTag(string name)
        {
            using (var ctx = new StackOverflowContext(_connectionString))
            {
                return ctx.Tags.FirstOrDefault(t => t.Name == name);
            }
        }
        private Tag GetTag(int id)
        {
            using (var ctx = new StackOverflowContext(_connectionString))
            {
                return ctx.Tags.FirstOrDefault(t => t.Id == id);
            }
        }
        public List<Tag> GetTags(int questionId)
        {
            using (var ctx = new StackOverflowContext(_connectionString))
            {
                List<QuestionsTags> qt = ctx.QuestionsTags.Where(q => q.QuestionId == questionId).ToList();
                List<Tag> t = new List<Tag>();
                foreach(QuestionsTags q in qt)
                {
                    Tag tag = GetTag(q.TagId);
                    t.Add(tag);
                }
                return t;
            }
        }
        private int AddTag(string name)
        {
            using (var ctx = new StackOverflowContext(_connectionString))
            {
                var tag = new Tag { Name = name };
                ctx.Tags.Add(tag);
                ctx.SaveChanges();
                return tag.Id;
            }
        }
        #endregion
        #region Answer
        public void AddAnswer(string text, int questionid, int userid)
        {
            Answer answer = new Answer
            {
                Text = text,
                QuestionId = questionid,
                UserId = userid,
            };
            using (var ctx = new StackOverflowContext(_connectionString))
            {
                ctx.Answers.Add(answer);
                ctx.SaveChanges();
            }

            }
        public List<Answer> GetAnswers (int questionId)
        {
            using (var ctx = new StackOverflowContext(_connectionString))
            {
                return ctx.Answers.Where(a => a.QuestionId == questionId).ToList();
            }
            }
            #endregion
        }
    public class StackOverflowContext : DbContext
        {
            private string _connectionString;

            public StackOverflowContext(string connectionString)
            {
                _connectionString = connectionString;
            }
            public DbSet<Question> Questions { get; set; }
            public DbSet<Tag> Tags { get; set; }
            public DbSet<QuestionsTags> QuestionsTags { get; set; }
            public DbSet<Like> Likes { get; set; }
            public DbSet<User> Users { get; set; }
            public DbSet<Answer> Answers { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseSqlServer(_connectionString);
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<QuestionsTags>()
                   .HasKey(qt => new { qt.QuestionId, qt.TagId });


                modelBuilder.Entity<QuestionsTags>()
                    .HasOne(qt => qt.Question)
                    .WithMany(q => q.QuestionsTags)
                    .HasForeignKey(q => q.QuestionId);


                modelBuilder.Entity<QuestionsTags>()
                    .HasOne(qt => qt.Tag)
                    .WithMany(t => t.QuestionsTags)
                    .HasForeignKey(q => q.TagId);

                modelBuilder.Entity<Like>()
                    .HasKey(uq => new { uq.QuestionId, uq.UserId });


                modelBuilder.Entity<Like>()
                    .HasOne(uq => uq.Question)
                    .WithMany(q => q.Likes)
                    .HasForeignKey(q => q.QuestionId);


                modelBuilder.Entity<Like>()
                    .HasOne(uq => uq.User)
                    .WithMany(u => u.Likes)
                    .HasForeignKey(q => q.UserId);
            }
        }
        public class StackOverflowContextFactory : IDesignTimeDbContextFactory<StackOverflowContext>
        {
            public StackOverflowContext CreateDbContext(string[] args)
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), $"..{Path.DirectorySeparatorChar}HW60_StackOverflow_May2"))
                    .AddJsonFile("appsettings.json")
                    .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true).Build();

                return new StackOverflowContext(config.GetConnectionString("ConStr"));
            }
        }

    }

//public class QuestionsRepository
//{
//    private string _connectionString;

//    public QuestionsRepository(string connectionString)
//    {
//        _connectionString = connectionString;
//    }



//    