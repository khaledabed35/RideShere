using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.SignalR;
using Nest;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.Helper
{
    public class RideHub:Hub
    {
        public async Task joinTrip(Guid tripid)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, tripid.ToString());
        }
    }
}
