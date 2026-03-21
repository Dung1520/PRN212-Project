using BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface ISlotService
    {
        List<Slot> GetAllSlots();
        Slot? GetSlotById(int id);
    }
}
