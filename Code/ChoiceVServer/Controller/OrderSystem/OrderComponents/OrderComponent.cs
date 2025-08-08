using AltV.Net.Data;
using ChoiceVServer.Controller.OrderSystem.OrderComponents.OrderItems;
using ChoiceVServer.Model.Database;
using ChoiceVServer.Model.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChoiceVServer.Controller.OrderController;

namespace ChoiceVServer.Controller.OrderSystem {
    public abstract class OrderComponent {
        public int RelativeId { get; protected set; }
        public int ShipId { get; protected set; }
        public int? ContainerId { get; protected set; }
        public string Name { get; protected set; }
        public string Category { get; protected set; }
        public int OrderCompany { get; protected set; }
        public int? ShippingCompany { get; protected set; }
        public OrderType OrderType { get; protected set; }
        public int Amount { get; protected set; }
        public string Data { get; protected set; }

        public abstract Menu harborMasterMenu(OrderShip ship);
        public abstract Menu containerMenu();

        public static OrderComponent createOrderComponent(orderitem orderItem) {
            switch((OrderType)orderItem.orderType) {
                case OrderType.Item:
                    return new OrderItem(orderItem);
                case OrderType.VehicleSpecificItem:
                    return new VehicleSpecificOrderItem(orderItem);
                case OrderType.Vehicle:
                    return new OrderVehicle(orderItem);
                case OrderType.Clothing:
                    return new OrderClothingItem(orderItem);
                    case OrderType.ClothingProp:
                    return new OrderClothingPropItem(orderItem);
                default:
                    return null;
            }
        }

        internal static string getDataFromCef(IDOSBoughtItem item, OrderableOption option) {
            switch((OrderType)item.type) {
                case OrderType.Item:
                    if(option == null) {
                        return $"{item.configId}";
                    } else {
                        return $"{item.configId}#{option.data}";
                    }
                case OrderType.VehicleSpecificItem:
                    return $"{item.configId}#{option.data}";
                case OrderType.Vehicle:
                    return $"{item.configId}#{option.data}";
                case OrderType.ClothingProp:
                case OrderType.Clothing:
                    return $"{item.configId}#{item.additionalInfo}";
                default:
                    return null;
            }
        }

        public void setContainerId(int id) {
            ContainerId = id;
        }

        public void setShippingCompany(int id) {
            ShippingCompany = id;
        }

        public void addAmount(int amount) {
            Amount += amount;
        }

        public bool Equals(OrderComponent component) {
            return component.OrderCompany == OrderCompany && component.OrderType == OrderType && component.Data == Data;
        }
    }
}
