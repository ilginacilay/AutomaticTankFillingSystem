using System;
using System.Threading;

namespace AutomaticTankFillingSystem
{
    class Program
    {
        // ==============================
        // SİSTEM DEĞİŞKENLERİ
        // ==============================

        static bool systemRunning = false;
        static bool pumpRunning = false;

        static int tankLevel = 30;

        static bool emergencyStop = false;

        static int fillStartLevel = 0;

        static bool lowLevelSensor = false;
        static bool highLevelSensor = false;


        // ==============================
        // ANA PROGRAM
        // ==============================

        static void Main(string[] args)
        {
            Console.WriteLine("======================================");
            Console.WriteLine("      OTOMATİK TANK DOLUM SİSTEMİ");
            Console.WriteLine("======================================");

            while (true)
            {
                // Sensörleri güncelle
                UpdateSensors();

                Console.WriteLine();
                Console.WriteLine($"Tank Seviyesi       : %{tankLevel}");
                Console.WriteLine($"Pompa Durumu        : {(pumpRunning ? "ÇALIŞIYOR" : "DURUYOR")}");
                Console.WriteLine($"Sistem Durumu       : {(systemRunning ? "AKTİF" : "DURDU")}");
                Console.WriteLine($"Low Level Sensor    : {(lowLevelSensor ? "AKTİF" : "PASİF")}");
                Console.WriteLine($"High Level Sensor   : {(highLevelSensor ? "AKTİF" : "PASİF")}");
                Console.WriteLine($"Emergency Stop      : {(emergencyStop ? "AKTİF" : "PASİF")}");

                Console.WriteLine();
                Console.WriteLine("1 - Sistemi Başlat");
                Console.WriteLine("2 - Sistemi Durdur");
                Console.WriteLine("3 - Tank Seviyesini Değiştir");
                Console.WriteLine("4 - Emergency Stop");
                Console.WriteLine("5 - Emergency Stop Reset");
                Console.WriteLine("6 - Çıkış");

                Console.Write("Seçiminiz: ");
                string? choice = Console.ReadLine();

                switch (choice)
                {
                    // ==============================
                    // 1 - SİSTEMİ BAŞLAT
                    // ==============================

                    case "1":

                        if (emergencyStop)
                        {
                            Console.WriteLine();
                            Console.WriteLine("Emergency Stop aktif!");
                            Console.WriteLine("Önce Emergency Stop Reset yapmalısınız.");
                            break;
                        }

                        systemRunning = true;

                        Console.WriteLine();
                        Console.WriteLine(
                            $"[EVENT] Sistem başlatıldı - {DateTime.Now:HH:mm:ss}"
                        );

                        UpdateSensors();
                        ControlPump();

                        Console.WriteLine();
                        Console.WriteLine("Sistem başlatıldı.");

                        if (pumpRunning)
                        {
                            CheckAlarms();

                            Console.WriteLine("Pompa çalışıyor...");

                            Console.WriteLine(
                                $"[EVENT] Pompa çalıştı - {DateTime.Now:HH:mm:ss}"
                            );
                        }
                        else
                        {
                            Console.WriteLine("Tank seviyesi yeterli.");
                            Console.WriteLine("Pompa çalıştırılmadı.");
                        }


                        // ==============================
                        // OTOMATİK DOLUM BAŞLANGICI
                        // ==============================

                        fillStartLevel = tankLevel;

                        DateTime fillStartTime = DateTime.Now;

                        Console.WriteLine(
                            $"Dolum başlangıç zamanı: {fillStartTime:HH:mm:ss}"
                        );

                        Console.WriteLine(
                            $"Dolum başlangıç seviyesi: %{fillStartLevel}"
                        );


                        // ==============================
                        // OTOMATİK DOLUM DÖNGÜSÜ
                        // ==============================

                        while (
                            systemRunning &&
                            pumpRunning &&
                            !emergencyStop &&
                            tankLevel < 100
                        )
                        {
                            // Klavyeden Emergency Stop kontrolü
                            if (Console.KeyAvailable)
                            {
                                char key = Console.ReadKey(true).KeyChar;

                                if (key == '4')
                                {
                                    emergencyStop = true;
                                    systemRunning = false;
                                    pumpRunning = false;

                                    Console.WriteLine();
                                    Console.WriteLine("!!! EMERGENCY STOP !!!");
                                    Console.WriteLine("Acil durdurma aktif!");
                                    Console.WriteLine("Sistem durduruldu.");
                                    Console.WriteLine("Pompa kapatıldı.");

                                    Console.WriteLine(
                                        $"[EVENT] Emergency Stop aktif - {DateTime.Now:HH:mm:ss}"
                                    );

                                    break;
                                }
                            }


                            // ==============================
                            // TANK DOLUMU
                            // ==============================

                            tankLevel += 10;

                            if (tankLevel > 100)
                            {
                                tankLevel = 100;
                            }


                            // Sensörleri güncelle
                            UpdateSensors();

                            // Pompa kontrolü
                            ControlPump();

                            Console.WriteLine(
                                $"Tank doluyor... Seviye: %{tankLevel}"
                            );


                            // ==============================
                            // HIGH LEVEL SENSOR
                            // ==============================

                            if (highLevelSensor)
                            {
                                Console.WriteLine();

                                CheckAlarms();

                                DateTime fillEndTime = DateTime.Now;

                                TimeSpan fillDuration =
                                    fillEndTime - fillStartTime;

                                Console.WriteLine("HIGH LEVEL SENSOR AKTİF!");
                                Console.WriteLine(
                                    "Pompa otomatik olarak durduruldu."
                                );

                                Console.WriteLine(
                                    $"Dolum süresi: {fillDuration.TotalSeconds:F1} saniye"
                                );

                                Console.WriteLine(
                                    $"Dolum özeti: %{fillStartLevel} → %{tankLevel}"
                                );

                                Console.WriteLine(
                                    $"[EVENT] Pompa durdu - {DateTime.Now:HH:mm:ss}"
                                );

                                break;
                            }

                            Thread.Sleep(1000);
                        }


                        // ==============================
                        // TANK %100 OLURSA
                        // ==============================

                        if (tankLevel >= 100)
                        {
                            tankLevel = 100;
                            pumpRunning = false;

                            Console.WriteLine();
                            Console.WriteLine("TANK DOLDU!");
                            Console.WriteLine(
                                "Pompa otomatik olarak durduruldu."
                            );
                        }

                        break;


                    // ==============================
                    // 2 - SİSTEMİ DURDUR
                    // ==============================

                    case "2":

                        systemRunning = false;
                        pumpRunning = false;

                        Console.WriteLine();
                        Console.WriteLine("Sistem durduruldu.");
                        Console.WriteLine("Pompa kapatıldı.");

                        Console.WriteLine(
                            $"[EVENT] Sistem durduruldu - {DateTime.Now:HH:mm:ss}"
                        );

                        break;


                    // ==============================
                    // 3 - TANK SEVİYESİNİ DEĞİŞTİR
                    // ==============================

                    case "3":

                        Console.Write(
                            "Yeni tank seviyesini giriniz (%0-100): "
                        );

                        string? levelInput = Console.ReadLine();

                        if (int.TryParse(levelInput, out int newLevel))
                        {
                            // Eski seviyeyi kaydet
                            int oldLevel = tankLevel;

                            if (newLevel >= 0 && newLevel <= 100)
                            {
                                tankLevel = newLevel;

                                Console.WriteLine();

                                Console.WriteLine(
                                    $"[EVENT] Tank seviyesi değiştirildi: " +
                                    $"%{oldLevel} → %{newLevel} - " +
                                    $"{DateTime.Now:HH:mm:ss}"
                                );

                                UpdateSensors();

                                Console.WriteLine();

                                Console.WriteLine(
                                    $"Tank seviyesi %{tankLevel} olarak ayarlandı."
                                );
                            }
                            else
                            {
                                Console.WriteLine();
                                Console.WriteLine(
                                    "Tank seviyesi 0 ile 100 arasında olmalıdır."
                                );
                            }
                        }
                        else
                        {
                            Console.WriteLine();
                            Console.WriteLine(
                                "Geçersiz tank seviyesi!"
                            );
                        }

                        break;


                    // ==============================
                    // 4 - EMERGENCY STOP
                    // ==============================

                    case "4":

                        emergencyStop = true;
                        systemRunning = false;
                        pumpRunning = false;

                        Console.WriteLine();
                        Console.WriteLine("!!! EMERGENCY STOP !!!");
                        Console.WriteLine("Acil durdurma aktif!");
                        Console.WriteLine("Sistem durduruldu.");
                        Console.WriteLine("Pompa kapatıldı.");

                        Console.WriteLine(
                            $"[EVENT] Emergency Stop aktif - {DateTime.Now:HH:mm:ss}"
                        );

                        break;


                    // ==============================
                    // 5 - EMERGENCY STOP RESET
                    // ==============================

                    case "5":

                        emergencyStop = false;
                        systemRunning = false;
                        pumpRunning = false;

                        Console.WriteLine();
                        Console.WriteLine(
                            "Emergency Stop sıfırlandı."
                        );

                        Console.WriteLine(
                            "Sistem yeniden başlatılmaya hazır."
                        );

                        Console.WriteLine(
                            $"[EVENT] Emergency Stop resetlendi - {DateTime.Now:HH:mm:ss}"
                        );

                        break;


                    // ==============================
                    // 6 - ÇIKIŞ
                    // ==============================

                    case "6":

                        Console.WriteLine();
                        Console.WriteLine(
                            "Program kapatılıyor..."
                        );

                        return;


                    // ==============================
                    // GEÇERSİZ SEÇİM
                    // ==============================

                    default:

                        Console.WriteLine();
                        Console.WriteLine(
                            "Geçersiz seçim!"
                        );

                        break;
                }
            }
        }


        // ==============================
        // SENSÖRLERİ GÜNCELLE
        // ==============================

        static void UpdateSensors()
        {
            // Low Level Sensor
            if (tankLevel <= 20)
            {
                lowLevelSensor = true;
            }
            else
            {
                lowLevelSensor = false;
            }


            // High Level Sensor
            if (tankLevel >= 90)
            {
                highLevelSensor = true;
            }
            else
            {
                highLevelSensor = false;
            }
        }


        // ==============================
        // POMPA KONTROLÜ
        // ==============================

        static void ControlPump()
        {
            if (emergencyStop)
            {
                pumpRunning = false;
            }
            else if (highLevelSensor)
            {
                pumpRunning = false;
            }
            else if (lowLevelSensor)
            {
                pumpRunning = true;
            }
        }


        // ==============================
        // ALARM KONTROLÜ
        // ==============================

        static void CheckAlarms()
        {
            if (lowLevelSensor)
            {
                Console.WriteLine("LOW LEVEL ALARM!");
                Console.WriteLine("Tank seviyesi düşük.");
            }
            else if (highLevelSensor)
            {
                Console.WriteLine("HIGH LEVEL WARNING!");
                Console.WriteLine("Tank seviyesi yüksek.");
            }
        }
    }
}