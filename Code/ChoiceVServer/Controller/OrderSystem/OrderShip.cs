using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChoiceVServer.Controller.OrderSystem {
    public class OrderShip {
        public DateTime ArriveTime;
        public List<OrderComponent> Components;
        public DateTime CreateDate;
        public int Id;
        public int ItemRelativeId;
        public DateTime LeaveTime;
        public ShipState ShipState;
        public DateTime StartTime;

        public OrderShip(int id, int itemRelativeId, ShipState shipState, DateTime createDate, DateTime startTime, DateTime arriveTime, DateTime leaveTime) {
            Id = id;
            ItemRelativeId = itemRelativeId;
            ShipState = shipState;
            CreateDate = createDate;
            StartTime = startTime;
            ArriveTime = arriveTime;
            LeaveTime = leaveTime;
            Components = new List<OrderComponent>();
        }

        public void addComponent(OrderComponent component) {
            Components.Add(component);
        }

        public void removeComponent(OrderComponent component) {
            Components.Remove(component);
        }

        public void setState(ShipState state) {
            ShipState = state;
        }
    }
}
