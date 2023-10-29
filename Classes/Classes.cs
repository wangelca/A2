using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;
using System.Reflection.PortableExecutable;
using static Traveless.Classes.Reservation;


namespace Traveless.Classes
{
    public class Flight
    {
        public string FlightCode { get; set; }
        public string Airline { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public DateTime Time { get; set; } // Change the data type to DateTime
        public decimal Cost { get; set; }
        public int AvailableSeats { get; set; }
        public bool IsFullyBooked => AvailableSeats <= 0;

    }

    public class Airport
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }
    public class Reservation
    {
        public string ReservationCode { get; set; }
        public Flight Flight { get; set; }
        public string TravelerName { get; set; }
        public string Citizenship { get; set; }
        public bool IsActive { get; set; }

        public Reservation(string reservationCode, Flight flight, string travelerName, string citizenship)
        {
            ReservationCode = reservationCode;
            Flight = flight;
            TravelerName = travelerName;
            Citizenship = citizenship;
            IsActive = true;
        }
        public class FlightMap : ClassMap<Flight>
        {
            public FlightMap()
            {
                Map(f => f.FlightCode).Name("FlightCode");
                Map(f => f.Airline).Name("Airline");
                Map(f => f.Origin).Name("Origin");
                Map(f => f.Destination).Name("Destination");
                Map(f => f.Time).Name("Time");
                Map(f => f.Cost).Name("Cost");
                Map(f => f.AvailableSeats).Name("AvailableSeats");
            }
        }
    }
    public class ReservationManager
    {
        private List<Reservation> reservations;
        private List<Flight> flights;

        public ReservationManager(string flightsCsvPath)
        {
            reservations = LoadReservationsFromFile();
            CsvDataLoader dataLoader = new CsvDataLoader();
            flights = dataLoader.LoadFlightsFromCsv(flightsCsvPath);
        }

        public Reservation MakeReservation(Flight flight, string travelerName, string citizenship)
        {
            if (flight.IsFullyBooked)
            {
                throw new Exception("The flight is fully booked. Reservation cannot be made.");
            }

            if (string.IsNullOrEmpty(travelerName) || string.IsNullOrEmpty(citizenship))
            {
                throw new ArgumentException("Please provide the traveler's name and citizenship.");
            }

            string reservationCode = GenerateReservationCode();
            var reservation = new Reservation(reservationCode, flight, travelerName, citizenship);
            reservations.Add(reservation);
            flight.AvailableSeats--;

            SaveReservationsToFile();

            return reservation;
        }

        public List<Reservation> FindReservations(string reservationCode, string airline, string travelerName)
        {
            return reservations.Where(r => r.ReservationCode == reservationCode ||
                                           r.Flight.Airline == airline ||
                                           r.TravelerName == travelerName).ToList();
        }

        public void UpdateReservation(Reservation reservation, string travelerName, string citizenship, bool isActive)
        {
            if (reservation == null)
            {
                throw new ArgumentNullException("Please provide a valid reservation.");
            }

            if (string.IsNullOrEmpty(travelerName) || string.IsNullOrEmpty(citizenship))
            {
                throw new ArgumentException("Please provide the traveler's name and citizenship.");
            }

            reservation.TravelerName = travelerName;
            reservation.Citizenship = citizenship;
            reservation.IsActive = isActive;

            SaveReservationsToFile();
        }

        private string GenerateReservationCode()
        {
            return Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
        }

        private List<Reservation> LoadReservationsFromFile()
        {
            if (File.Exists("reservations.dat"))
            {
                using (FileStream fileStream = new FileStream("reservations.dat", FileMode.Open))
                {
                    var binaryFormatter = new BinaryFormatter();
                    return (List<Reservation>)binaryFormatter.Deserialize(fileStream);
                }
            }
            else
            {
                return new List<Reservation>();
            }
        }

        private void SaveReservationsToFile()
        {
            using (FileStream fileStream = new FileStream("reservations.dat", FileMode.Create))
            {
                var binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(fileStream, reservations);
            }
        }

        public List<Flight> GetFlightsByCriteria(string from, string to, string day)
        {
            if (flights == null)
            {
                throw new InvalidOperationException("Flights collection is not initialized.");
            }

            var targetDayOfWeek = GetDayOfWeekFromInput(day);

            return flights
                .Where(f =>
                    f.Origin == from
                    && f.Destination == to
                    && f.Time.DayOfWeek == targetDayOfWeek)
                .ToList();
        }

        private DayOfWeek GetDayOfWeekFromInput(string day)
        {
            if (Enum.TryParse(day, true, out DayOfWeek result))
            {
                return result;
            }

            switch (day.ToLower())
            {
                case "monday":
                    return DayOfWeek.Monday;
                case "tuesday":
                    return DayOfWeek.Tuesday;
                case "wednesday":
                    return DayOfWeek.Wednesday;
                case "thursday":
                    return DayOfWeek.Thursday;
                case "friday":
                    return DayOfWeek.Friday;
                case "saturday":
                    return DayOfWeek.Saturday;
                case "sunday":
                    return DayOfWeek.Sunday;
                default:
                    throw new ArgumentException("Invalid day input: " + day);
            }
        }
        public class CsvDataLoader
        {
            public List<Flight> LoadFlightsFromCsv(string filePath)
            {
                using var reader = new StreamReader("C:\\Users\\angel\\Desktop\\A2\\Traveless\\Classes\\flights.csv");
                using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture));
                csv.Context.RegisterClassMap<FlightMap>();
                var flights = csv.GetRecords<Flight>().ToList();
                return flights;
            }

            public List<Airport> LoadAirportsFromCsv(string filePath)
            {
                using var reader = new StreamReader("C:\\Users\\angel\\Desktop\\A2\\Traveless\\Classes\\airports.csv");
                using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture));
                var airports = csv.GetRecords<Airport>().ToList();
                return airports;
            }
        }
    }



    internal class AnyReservationManager
    {
    }
}




    