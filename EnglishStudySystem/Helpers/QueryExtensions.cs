using System.Linq;
using EnglishStudySystem.Models; // Đảm bảo namespace này khớp với dự án của bạn

namespace EnglishStudySystem.Helpers
{
    public static class QueryExtensions
    {
        // Phương thức mở rộng để chỉ lấy các đối tượng chưa bị xóa mềm
        public static IQueryable<T> WhereNotDeleted<T>(this IQueryable<T> query) where T : class, ISoftDeletable
        {
            return query.Where(e => !e.IsDeleted);
        }

        // Phương thức mở rộng để chỉ lấy các đối tượng đã bị xóa mềm
        public static IQueryable<T> WhereDeleted<T>(this IQueryable<T> query) where T : class, ISoftDeletable
        {
            return query.Where(e => e.IsDeleted);
        }
    }
}