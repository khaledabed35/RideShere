using BLL.Specification.Class;
using DAL.Models;
using System;
using System.Linq.Expressions;

namespace BLL.Specifications // أو حسب المسار لديك
{
    public class NotificationSpecification : Specification<Notification>
    {
        // جلب كل إشعارات المستخدم مرتبة من الأحدث للأقدم
        public NotificationSpecification(Guid userId)
            : base(n => n.UserId == userId)
        {
            AddOrderByDescending(n => n.CreatedAt);
        }

        // جلب الإشعارات غير المقروءة فقط (اختياري)
        public NotificationSpecification(Guid userId, bool unreadOnly)
            : base(n => n.UserId == userId && (!unreadOnly || !n.IsRead))
        {
            AddOrderByDescending(n => n.CreatedAt);
        }
    }
}