using System;
using System.Collections.Generic;
using System.Linq;
using MenuLib.API;
using MenuLib.API.Factories;
using SpeedrunMod.Practice;

namespace SpeedrunMod.Menus;

internal static class ChapterSelectorMenu
{
    private const int PageSize = 7;

    internal static void CreateMenu(PauseMenu menu)
    {
        var playableChapters = Enum.GetValues(typeof(GameChapter))
            .Cast<GameChapter>()
            .Where(chapter => chapter.IsPlayable())
            .ToArray();
        
        if (playableChapters.Length == 0)
        {
            return;
        }

        var index = 0;
        var chapterPages = playableChapters.Chunk(PageSize).ToArray();
        var pages = new List<PauseMenuPage>(chapterPages.Length);
        foreach (var chapters in chapterPages)
        {
            var page = new PauseMenuPageFactory()
                .SetParentMenu(menu)
                .FromExistingMenu(menu, $"ChapterSelectorMenu_{index + 1}")
                .SetTitle($"CHAPTERS {index + 1}/{chapterPages.Length}")
                .Build();

            pages.Add(page);

            foreach (var chapter in chapters)
            {
                var selectedChapter = chapter;
                new PauseMenuOptionFactory()
                    .SetObjectName($"ChapterSelectorMenuOption_{selectedChapter}")
                    .SetName(selectedChapter.ToDisplayName())
                    .SetParent(page)
                    .SetOnClick(() => ChapterSelector.Load(selectedChapter, fullReload: true))
                    .CloseOnClick()
                    .SetOrder(-1)
                    .Build();
            }

            index++;
        }

        for (var pageIndex = 0; pageIndex < pages.Count; pageIndex++)
        {
            pages[pageIndex]
                .SetPreviousPage(pageIndex > 0 ? pages[pageIndex - 1] : null)
                .SetNextPage(pageIndex < pages.Count - 1 ? pages[pageIndex + 1] : null)
                .Build();
        }

        var chapterSubmenu = pages[0];

        new PauseMenuOptionFactory()
            .SetObjectName("ChapterSelectorMenuOption")
            .SetName("CHAPTERS")
            .SetParent(menu)
            .SetNextLocation(chapterSubmenu)
            .SetOrder(-1)
            .Build();
    }
}
