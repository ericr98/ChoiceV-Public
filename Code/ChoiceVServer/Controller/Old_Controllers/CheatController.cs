namespace ChoiceVServer.Controller {
    //class CheatController : ChoiceVScript {
    //    public CheatController() {
    //        //Weapon Cheating handled in WeaponController
    //        EventController.addEvent("ANTICHEAT_INVINCIBLE", onPlayerInvincible);
    //        EventController.addEvent("ANTICHEAT_BAN", onPlayerBan);
    //        EventController.PlayerConnectedDelegate += onConnect;
    //    }

    //    private bool onPlayerBan(IPlayer player, string eventName, object[] args) {
    //        player.ban("Cheating");
    //        return true;
    //    }

    //    private void onConnect(IPlayer player, string reason) {
    //        player.emitClientEvent("DISABLE_INVINCIBLE");
    //    }

    //    private bool onPlayerInvincible(IPlayer player, string eventName, object[] args) {
    //        if (player.getAdminLevel() >= 1) {
    //            return true;
    //        } else {
    //            player.ban("Cheating");
    //        }

    //        return true;
    //    }
    //}
}
