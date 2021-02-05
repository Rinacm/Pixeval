#region Copyright (C) 2019-2020 Dylech30th. All rights reserved.
// Pixeval - A Strong, Fast and Flexible Pixiv Client
// Copyright (C) 2019-2020 Dylech30th
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mako.Model;
using NUnit.Framework;

namespace Mako.Test
{
    [Order(4)]
    public class RankingAsyncEnumerableTest
    {
        private static MakoClient MakoClient => Global.MakoClient;

        private static async Task TryGetRanking(RankOption rankOption)
        {
            var list = new List<Illustration>();
            var enumerable = MakoClient.Ranking(rankOption, DateTime.Today - TimeSpan.FromDays(2));
            await foreach (var illustration in enumerable)
            {
                if (illustration == null)
                    continue;

                illustration.Print();
                enumerable.InsertTo(list, illustration);
            }

            Console.WriteLine($"Size: {list.Count}");
            Assert.IsNotEmpty(list);
            Assert.IsTrue(list.IsDistinct(illust => illust.Id));
        }

        [Test, Parallelizable]
        public async Task RankingTestWithOptionDay()
        {
            await TryGetRanking(RankOption.Day);
        }

        [Test, Parallelizable]
        public async Task RankingTestWithOptionWeek()
        {
            await TryGetRanking(RankOption.Week);
        }

        [Test, Parallelizable]
        public async Task RankingTestWithOptionMonth()
        {
            await TryGetRanking(RankOption.Month);
        }

        [Test, Parallelizable]
        public async Task RankingTestWithOptionDayMale()
        {
            await TryGetRanking(RankOption.DayMale);
        }

        [Test, Parallelizable]
        public async Task RankingTestWithOptionDayFemale()
        {
            await TryGetRanking(RankOption.DayFemale);
        }

        [Test, Parallelizable]
        public async Task RankingTestWithOptionDayManga()
        {
            await TryGetRanking(RankOption.DayManga);
        }

        [Test, Parallelizable]
        public async Task RankingTestWithOptionWeekManga()
        {
            await TryGetRanking(RankOption.WeekManga);
        }

        [Test, Parallelizable]
        public async Task RankingTestWithOptionWeekOriginal()
        {
            await TryGetRanking(RankOption.WeekOriginal);
        }

        [Test, Parallelizable]
        public async Task RankingTestWithOptionWeekRookie()
        {
            await TryGetRanking(RankOption.WeekRookie);
        }

        [Test, Parallelizable]
        public async Task RankingTestWithOptionDayR18()
        {
            await TryGetRanking(RankOption.DayR18);
        }

        [Test, Parallelizable]
        public async Task RankingTestWithOptionDayMaleR18()
        {
            await TryGetRanking(RankOption.DayMaleR18);
        }

        [Test, Parallelizable]
        public async Task RankingTestWithOptionDayFemaleR18()
        {
            await TryGetRanking(RankOption.DayFemaleR18);
        }

        [Test, Parallelizable]
        public async Task RankingTestWithOptionWeekR18()
        {
            await TryGetRanking(RankOption.WeekR18);
        }

        [Test, Parallelizable]
        public async Task RankingTestWithOptionWeekR18G()
        {
            await TryGetRanking(RankOption.WeekR18G);
        }
    }
}