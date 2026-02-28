using Microsoft.AspNetCore.SignalR;

namespace EmployeeLeaveManagement.Hubs
{
    public class NotificationHub : Hub
    {
        
        public async Task SendNotificationToUser(string userId, string message, string type)
        {
            await Clients.User(userId).SendAsync("ReceiveNotification", message, type);
        }

     
        public async Task SendNotificationToGroup(string groupName, string message, string type)
        {
            await Clients.Group(groupName).SendAsync("ReceiveNotification", message, type);
        }

       
        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

      
        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

       
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

     
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}