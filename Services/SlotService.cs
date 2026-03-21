using BusinessObjects;
using Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class SlotService : ISlotService
    {
        private readonly ISlotRepository _slotRepository;

        public SlotService(ISlotRepository slotRepository)
        {
            _slotRepository = slotRepository;
        }

        public List<Slot> GetAllSlots()
        {
            return _slotRepository.GetAllSlots();
        }

        public Slot? GetSlotById(int id)
        {
            return _slotRepository.GetSlotById(id);
        }
    }
}
