using System.Diagnostics;
using System.Globalization;
using System.Linq.Expressions;
using System.Text;
using DB;
using DB.Models;
using IntervalLearningApi.Models.ByUser;

using Microsoft.AspNetCore.Mvc;

namespace IntervalLearningApi.Controllers
{
    [Route("api/reports")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly ApplicationContext db;

        public ReportsController(ApplicationContext db)
        {
            this.db = db;
        }

        //https://localhost:7249/api/reports?scheduleUserId=1&scheduleId=8&userId=1&phaseIndex=7
        [HttpGet]
        public FileStreamResult GetStatistic(
            long scheduleUserId,
            int scheduleId,
            int? phaseIndex,
            long userId = 0,
            Format passedTimeFormat = Format.Hours)

        
        {
            var stream = new MemoryStream();
            var textWriter = new StreamWriter(stream, Encoding.UTF8);

            var query = db.Remembers.AsQueryable();

            if (scheduleUserId != 0)
                query = query.Where(r => r.ParentRepeatsScheduleUserId == scheduleUserId);

            if (scheduleId != 0)
                query = query.Where(r => r.ParentRepeatsScheduleId == scheduleId);

            if (userId != 0)
                query = query.Where(r => r.ParentUserId == userId);

            var allRemembers = query.ToList();

            var uniqueKeyToCardRemembers = allRemembers
                .GroupBy(GetCardUniqueKey)
                .ToDictionary(g => g.Key, g => g.OrderBy(r => r.RepeatedDate).ToList());

            var phaseIndexToPassedTimeToRemembers = new Dictionary<int, Dictionary<int, List<RememberEntity>>>();

            foreach (var (_, remembers) in uniqueKeyToCardRemembers)
            {
                for (var i = 0; i < remembers.Count; i++)
                {
                    var remember = remembers[i];

                    if (phaseIndex.HasValue && remember.PhaseIndex != phaseIndex.Value)
                        continue;

                    var passedTimeFromLastStep = 0;

                    if (i != 0)
                    {
                        var passedTime = remember.RepeatedDate - remembers[i - 1].RepeatedDate;
                        passedTimeFromLastStep = ConvertTimeToFormat(passedTime, passedTimeFormat);
                    }
                    
                    Debug.Assert(passedTimeFromLastStep >= 0);

                    phaseIndexToPassedTimeToRemembers.TryAdd(remember.PhaseIndex,
                        new Dictionary<int, List<RememberEntity>>());

                    var phaseData = phaseIndexToPassedTimeToRemembers[remember.PhaseIndex];

                    phaseData.TryAdd(passedTimeFromLastStep, new List<RememberEntity>());
                    //phaseIndexToPassedTimeToRemembers.TryAdd(passedTimeFromLastStep, (null, new List<RememberEntity>()));

                    phaseData[passedTimeFromLastStep].Add(remember);
                }
            }

            textWriter.WriteLine(
                string.Join(",",
                "Phase Index",
                "Passed Time",
                "Total",
                "Good",
                "Medium",
                "Bad",
                "Percent of Good",
                "Percent of Bad"
            ));


            foreach (var (currentPhaseIndex, passedTimeToRemembers) in phaseIndexToPassedTimeToRemembers)
            {
                foreach (var (passedTime, remembers) in passedTimeToRemembers)
                {
                    var (good, medium, bad) = CountRemembersValues(remembers);

                    var total = remembers.Count;

                    textWriter.WriteLine(
                        string.Join(",",
                            currentPhaseIndex,
                            passedTime,
                            total,
                            good,
                            medium,
                            bad,
                            ((double)good / total * 100).ToString("##.##", CultureInfo.InvariantCulture),
                            ((double)bad / total * 100).ToString("##.##", CultureInfo.InvariantCulture))
                        );
                }
            }

            textWriter.Flush();
            stream.Seek(0, SeekOrigin.Begin);

            return File(stream, "text/plain", "il_statistic.csv");

            static string GetCardUniqueKey(RememberEntity r)
                => string.Join("-",
                    r.ParentRepeatsScheduleUserId,
                    r.ParentRepeatsScheduleId,
                    r.ParentUserId,
                    r.ParentCollectionId,
                    r.ParentCardId);
        }

        private (int good, int medium, int bad) CountRemembersValues(List<RememberEntity> remembers)
        {
            int good = 0, medium = 0, bad = 0;

            foreach (var r in remembers)
            {
                if (r.Weight >= .8)
                    good++;
                else if (r.Weight >= .4)
                    medium++;
                else
                    bad++;
            }

            return (good, medium, bad);
        }

        private string GetHeaderDataString()
        {
            return string.Join(",",
                "Schedule User Id",
                "Schedule Id",
                "User Id",
                "Collection Id",
                "Phase Index",
                "Weight",
                "Passed Time");
        }

        private string GetRememberDataString(RememberEntity r, double passedTimeFromLastStep)
        {
            return string.Join(",", 
                r.ParentRepeatsScheduleUserId,
                r.ParentRepeatsScheduleId,
                r.ParentUserId,
                r.ParentCollectionId,
                r.PhaseIndex,
                r.Weight,
                passedTimeFromLastStep);
        }

        private int ConvertTimeToFormat(TimeSpan passedTime, Format format)
        {
            return (int) Math.Round(format switch
            {
                Format.Minutes => passedTime.TotalMinutes,
                Format.Hours => passedTime.TotalHours,
                Format.Day => passedTime.TotalDays,
            });
        }

        public enum Format
        {
            Minutes = 1,
            Hours = 2,
            Day = 3,
        }
    }
}
