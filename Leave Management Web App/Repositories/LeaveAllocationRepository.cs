using Leave_Management_Web_App.Constants;
using Leave_Management_Web_App.Contracts;
using Leave_Management_Web_App.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Leave_Management_Web_App.Repositories
{
    public class LeaveAllocationRepository : GenericRepository<LeaveAllocation>, ILeaveAllocationRepository
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<Employee> userManager;
        private readonly ILeaveTypeRepository leaveTypeRepository;

        public LeaveAllocationRepository(ApplicationDbContext context, UserManager<Employee> userManager,
            ILeaveTypeRepository leaveTypeRepository) : base(context)
        {
            this.context = context;
            this.userManager = userManager;
            this.leaveTypeRepository = leaveTypeRepository;
        }

        public async Task<bool> AllocationExists(string employeeId, int leaveTypeId, int period)
        {
            return await context.LeaveAllocations.AnyAsync(q => q.EmployeeId == employeeId &&
                                                                q.LeaveTypeId == leaveTypeId &&
                                                                q.Period == period);
        }

        public async Task LeaveAllocation(int leaveTypeId)
        {
            var employees = await userManager.GetUsersInRoleAsync(Roles.User);
            var period = DateTime.Now.Year;
            var leaveType = await leaveTypeRepository.GetAsync(leaveTypeId);
            var allocations = new List<LeaveAllocation>();

            foreach (var employee in employees)
            {
                if (await AllocationExists(employee.Id, leaveTypeId, period))
                {
                    continue;
                }
                allocations.Add(new LeaveAllocation
                {
                    DateCreated = DateTime.Now.Date,
                    EmployeeId = employee.Id,
                    LeaveTypeId = leaveTypeId,
                    NumberOfDays = leaveType.DefaultDays,
                    Period = period
                });
            }

            await AddRangeAsync(allocations);
        }
    }
}
