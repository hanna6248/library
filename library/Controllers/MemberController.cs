using Microsoft.AspNetCore.Mvc;
using Npgsql;
using library.Data;
using library.Models;

namespace library.Controllers;

public class MembersController : Controller
{
    private readonly MemberRepository _members;

    public MembersController(MemberRepository members)
    {
        _members = members;
    }

    public async Task<IActionResult> Index()
    {
        var members = await _members.GetAllAsync();
        return View(members);
    }

    public IActionResult Create()
    {
        return View(new Member());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Member member)
    {
        if (!ModelState.IsValid)
        {
            return View(member);
        }

        await _members.AddAsync(member);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(long id)
    {
        var member = await _members.GetByIdAsync(id);

        if (member == null)
        {
            return NotFound();
        }

        return View(member);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Member member)
    {
        if (!ModelState.IsValid)
        {
            return View(member);
        }

        await _members.UpdateAsync(member);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(long id)
    {
        var member = await _members.GetByIdAsync(id);

        if (member == null)
        {
            return NotFound();
        }

        return View(member);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(long id)
    {
            try
            {
                // 1. Delete the member.
                await _members.DeleteAsync(id);
            }
            catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.ForeignKeyViolation)
            {
                // 3. Load the member again, to show them on the page.
                var member = await _members.GetByIdAsync(id);

                // 4. Check if there is no such member.
                if (member == null)
                {
                    // 5. Answer "not found".
                    return NotFound();
                }

                // 6. Set the message the page will show.
                ViewBag.Error = "This member has loans on record, so the database will not delete them.";

                // 7. Show the Delete page again, with the member.
                return View("Delete", member);
            }

            // 8. The member was deleted: go back to the list.
            return RedirectToAction(nameof(Index));
            }
    }
