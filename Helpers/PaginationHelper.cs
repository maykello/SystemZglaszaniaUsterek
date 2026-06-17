namespace SystemZglaszaniaUsterek.Helpers
{
    public static class PaginationHelper
    {
        public static List<int?> GetPageItems(int currentPage, int totalPages)
        {
            var result = new List<int?>();
            if (totalPages <= 0)
            {
                return result;
            }

            var current = Math.Clamp(currentPage, 1, totalPages);

            var pages = new SortedSet<int> { 1, totalPages, current };
            if (current - 1 >= 1)
            {
                pages.Add(current - 1);
            }
            for (var i = 1; i <= 5; i++)
            {
                var next = current + i;
                if (next <= totalPages)
                {
                    pages.Add(next);
                }
            }

            int? previous = null;
            foreach (var page in pages)
            {
                if (previous.HasValue && page - previous.Value > 1)
                {
                    result.Add(null);
                }
                result.Add(page);
                previous = page;
            }

            return result;
        }
    }
}
