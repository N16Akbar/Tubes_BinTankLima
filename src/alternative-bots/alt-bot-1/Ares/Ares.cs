using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class Ares : Bot {
    static void Main(string[] args) {
        new Ares().Start();
    }

    Ares(): base(BotInfo.FromFile("Ares.json")) {}

    public override void Run() {
        // Membuat radar dan gun bergerak terpisah dari body
        AdjustRadarForGunTurn = true;
        AdjustGunForBodyTurn = true;
        AdjustRadarForBodyTurn = true;

        // Warna setiap bagian dari tank
        BodyColor = Color.FromArgb(160, 30, 30);
        GunColor = Color.Black;
        RadarColor = Color.Red;
        BulletColor = Color.Orange;
        ScanColor = Color.Yellow;
        TracksColor = Color.DarkRed;

        // Selama tank berjalan,
        while (IsRunning) {
            // lakukan pemutaran radar untuk mencari musuh
            TurnRadarLeft(45);
        }
    }

    // Override bot tank saat menemukan musuh
    public override void OnScannedBot(ScannedBotEvent evt) {
        // Menyimpan jarak ke bot musuh
        double enemyDistance = DistanceTo(evt.X, evt.Y);

        // Mengunci radar ke bot musuh
        SetTurnRadarLeft(RadarBearingTo(evt.X, evt.Y));

        // Mengarahkan gun ke bot musuh
        double gunTurn = GunBearingTo(evt.X, evt.Y);
        SetTurnGunLeft(gunTurn);

        // Mengarahkan body langsung ke bot musuh untuk melakukan ramming
        SetTurnLeft(BearingTo(evt.X, evt.Y));

        // Selalu maju ke arah bot musuh
        SetForward(enemyDistance);

        // Menentukan besar peluru berdasarkan jarak ke bot musuh
        double firePower;

        if (enemyDistance < 120)
            firePower = 3.0;
        else if (enemyDistance < 300)
            firePower = 2.0;
        else
            firePower = 1.0;

        // Jika energi rendah, kurangi besar peluru agar energi tetap aman
        if (Energy < 30 && firePower > 1.0)
            firePower = 1.0;

        if (Energy < 15 && firePower > 0.7)
            firePower = 0.7;

        // Tembak jika gun sudah cukup mengarah dan tidak panas
        if (Math.Abs(gunTurn) < 10 && GunHeat == 0) {
            SetFire(firePower);
        }

        // Eksekusi semua perintah Set*()
        Go();
    }

    public override void OnHitBot(HitBotEvent evt) {
        // Mengarahkan body ke bot yang tertabrak
        SetTurnLeft(BearingTo(evt.X, evt.Y));

        // Tetap maju untuk menabrak bot musuh
        SetForward(DistanceTo(evt.X, evt.Y));

        // Mengarahkan gun ke bot yang tertabrak
        double gunTurn = GunBearingTo(evt.X, evt.Y);
        SetTurnGunLeft(gunTurn);

        // Tembak saat bot sedang sangat dekat
        if (Math.Abs(gunTurn) < 10 && GunHeat == 0) {
            SetFire(2.0);
        }

        // Eksekusi semua perintah Set*()
        Go();
    }

    public override void OnHitWall(HitWallEvent evt) {
        // Jika menabrak dinding, kembali ke tengah arena agar tidak tersangkut
        SetTurnLeft(CalcDeltaAngle(DirectionTo(ArenaWidth / 2, ArenaHeight / 2), Direction));
        SetForward(180);

        Go();
    }

    public override void OnHitByBullet(HitByBulletEvent evt) {
        // Saat terkena peluru, tetap bergerak agar tidak diam
        SetForward(120);

        Go();
    }

    public override void OnSkippedTurn(SkippedTurnEvent evt) {
        // Jika turn terlewat, tetap maju agar bot terus agresif
        SetForward(120);

        Go();
    }
}