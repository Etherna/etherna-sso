namespace Etherna.SSOServer.Domain.Models
{
    public class DailyStats : EntityModelBase<string>
    {
        // Constructors.
        public DailyStats(
            long activeUsersInLast30Days,
            long activeUsersInLast60Days,
            long activeUsersInLast180Days,
            long totalUsers)
        {
            ActiveUsersInLast30Days = activeUsersInLast30Days;
            ActiveUsersInLast60Days = activeUsersInLast60Days;
            ActiveUsersInLast180Days = activeUsersInLast180Days;
            TotalUsers = totalUsers;
        }
        protected DailyStats() { }

        // Properties.
        public virtual long ActiveUsersInLast30Days { get; protected set; }
        public virtual long ActiveUsersInLast60Days { get; protected set; }
        public virtual long ActiveUsersInLast180Days { get; protected set; }
        public virtual long TotalUsers { get; protected set; }
    }
}
