﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskBot.Core.Services
{
    internal interface IToDoReportService
    {
        Task<(int total, int completed, int active, DateTime generatedAt)> GetUserStats(Guid userId, CancellationToken ct);
    }
}
