using Microsoft.AspNetCore.Mvc;
using TaskManagerApi.Models;
namespace TaskManagerApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskController : ControllerBase{
        private static List<TaskItem> tasks = new List<TaskItem>{
            new TaskItem { Id = 1, Title = "Wash car", DueDate = "2025-01-01", Priority = "low", Status = "To do" },
            new TaskItem { Id = 2, Title = "Pay bills", DueDate = "2025-01-02", Priority = "medium", Status = "in progress" },
            new TaskItem { Id = 3, Title = "Call friend", DueDate = "2025-01-03", Priority = "high", Status = "done" },
        };
        
        [HttpPost]
        public ActionResult<TaskItem> PostTaskItem(TaskItem task){
            if (task == null){
                return BadRequest("Please provide task details.");
            } else if (string.IsNullOrEmpty(task.Title) || string.IsNullOrEmpty(task.Priority) || string.IsNullOrEmpty(task.Status)){
                return BadRequest("Please provide Title, Priority, and Status.");
            }

            task.Id = tasks.Max(m => m.Id) + 1;
            tasks.Add(task);
            return CreatedAtAction(nameof(GetTaskItem), new { id = task.Id }, task);
        }

        [HttpGet]
        public ActionResult<IEnumerable<TaskItem>> GetTaskItems(
            [FromQuery] string? priority, 
            [FromQuery] string? status,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1)
                return BadRequest("Page number must be greater than 0");
            if (pageSize < 1)
                return BadRequest("Page size must be greater than 0");

            var filteredTasks = tasks.AsQueryable();
            
            if (!string.IsNullOrEmpty(priority))
            {
                filteredTasks = filteredTasks.Where(t => t.Priority.Equals(priority, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(status))
            {
                filteredTasks = filteredTasks.Where(t => t.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
            }

            var paginatedTasks = filteredTasks
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var response = new
            {
                TotalItems = filteredTasks.Count(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                Tasks = paginatedTasks
            };

            return Ok(response);
        }
        
        [HttpGet("{id}")]
        public ActionResult<TaskItem> GetTaskItem(int id){
            var task = tasks.FirstOrDefault(m => m.Id == id);
            if (task == null){
                return NotFound("Could not find task with ID=" + id);
            }
            return task;
        }
        
        [HttpPut("{id}")]
        public IActionResult PutTaskItem(int id, TaskItem updatedTaskItem){
            if (updatedTaskItem == null){
                return BadRequest("Please provide task details.");
            }else if (string.IsNullOrEmpty(updatedTaskItem.Title) || string.IsNullOrEmpty(updatedTaskItem.Priority) || string.IsNullOrEmpty(updatedTaskItem.Status)){
                return BadRequest("Please provide Title, Priority, and Status.");
            }

            var task = tasks.FirstOrDefault(m => m.Id == id);
            if (task == null){
                return NotFound("Could not find task with ID=" + id);
            }

            task.Title = updatedTaskItem.Title;
            task.DueDate = updatedTaskItem.DueDate;
            task.Priority = updatedTaskItem.Priority;
            task.Status = updatedTaskItem.Status;
            return Ok("Updated task with ID=" + id);
        }
        
        [HttpDelete("{id}")]
        public IActionResult DeleteTaskItem(int id)
        {
            var task = tasks.FirstOrDefault(m => m.Id == id);
            if (task == null)
            {
                return NotFound("Could not find task with ID=" + id);
            }
            tasks.Remove(task);
            return Ok("Deleted task with ID=" + id);
        }
    }
}