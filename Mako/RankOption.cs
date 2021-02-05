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

using Mako.Util;

namespace Mako
{
    public enum RankOption
    {
        [EnumMetadata("day")]
        Day,

        [EnumMetadata("week")]
        Week,

        [EnumMetadata("month")]
        Month,

        [EnumMetadata("day_male")]
        DayMale,

        [EnumMetadata("day_female")]
        DayFemale,

        [EnumMetadata("day_manga")]
        DayManga,

        [EnumMetadata("week_manga")]
        WeekManga,

        [EnumMetadata("week_original")]
        WeekOriginal,

        [EnumMetadata("week_rookie")]
        WeekRookie,

        [EnumMetadata("day_r18")]
        DayR18,

        [EnumMetadata("day_male_r18")]
        DayMaleR18,

        [EnumMetadata("day_female_r18")]
        DayFemaleR18,

        [EnumMetadata("week_r18")]
        WeekR18,

        [EnumMetadata("week_r18g")]
        WeekR18G
    }
}