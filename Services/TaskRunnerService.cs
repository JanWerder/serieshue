

using System.Diagnostics;
using serieshue.Interfaces;
using serieshue.Models;

namespace serieshue.Services;

public class TaskRunnerService : ITaskRunnerService
{

    private readonly SeriesHueContext _context;

    public TaskRunnerService(SeriesHueContext context)
    {
        _context = context;
    }
    public void RunIMDBUpdate()
    {
       
    }
}