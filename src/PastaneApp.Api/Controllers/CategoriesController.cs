using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PastaneApp.Api.Dtos;
using PastaneApp.Core.Entities;
using PastaneApp.Core.Interfaces;

namespace PastaneApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public CategoriesController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetAll()
    {
        var categories = await _unitOfWork.Repository<Category>().GetAllAsync();

        return categories
            .Where(c => c.ShowOnHome || User.IsInRole("Admin"))
            .OrderBy(c => c.Id)
            .Select(ToDto)
            .ToList();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryDto>> GetById(int id)
    {
        var category = await _unitOfWork.Repository<Category>().GetByIdAsync(id);
        if (category is null || (!category.ShowOnHome && !User.IsInRole("Admin")))
        {
            return NotFound();
        }

        return ToDto(category);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CategoryDto>> Create(CategoryUpsertRequest request)
    {
        var category = new Category
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim()
        };

        await _unitOfWork.Repository<Category>().AddAsync(category);
        await _unitOfWork.CompleteAsync();

        return CreatedAtAction(nameof(GetById), new { id = category.Id }, ToDto(category));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, CategoryUpsertRequest request)
    {
        var category = await _unitOfWork.Repository<Category>().GetByIdAsync(id);
        if (category is null)
        {
            return NotFound();
        }

        category.Name = request.Name.Trim();
        category.Description = request.Description?.Trim();

        _unitOfWork.Repository<Category>().Update(category);
        await _unitOfWork.CompleteAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _unitOfWork.Repository<Category>().GetByIdAsync(id);
        if (category is null)
        {
            return NotFound();
        }

        var hasProducts = (await _unitOfWork.Repository<Product>().FindAsync(p => p.CategoryId == id)).Any();
        if (hasProducts)
        {
            return Conflict(new ProblemDetails { Title = "Bu kategoriye bağlı ürünler var, önce onları silin veya başka kategoriye taşıyın." });
        }

        _unitOfWork.Repository<Category>().Remove(category);
        await _unitOfWork.CompleteAsync();

        return NoContent();
    }

    private static CategoryDto ToDto(Category category) => new(category.Id, category.Name, category.Description);
}
