# 🚗 RideShere

A real-time ride-sharing backend system built with ASP.NET Core Web API.

---

## 📌 What is RideShere?

RideShere is an Uber-like ride-sharing platform that connects passengers with nearby online drivers in real time.

The passenger requests a ride by providing:
- 📍 Pickup location
- 📍 Drop-off location
- 💰 Initial estimated fare
- 💳 Payment method

Nearby drivers receive the ride request in real time and can submit their own fare offers.

The passenger can compare available offers based on:
- Driver name & rating
- Vehicle information
- Proposed fare
- Distance
- Estimated arrival time

The passenger then selects one driver and the trip begins.

---

## 🔄 Complete Ride Flow

### Flow
Passenger requests a ride → nearby drivers are discovered → drivers receive real-time notifications → drivers submit offers → passenger selects an offer → trip starts → trip is completed → payment is processed → passenger leaves a review.

---

## 🚗 Driver Verification

Drivers must be verified before they can receive ride requests.

**Registration → Upload Documents → Pending → Admin Review → Approved / Rejected**

Only approved drivers can go online and receive ride requests.

---

## 📍 Geolocation & Nearby Drivers

RideShere uses geographical coordinates to find nearby online drivers.

**Longitude + Latitude → Redis GEO → Radius Search → Nearby Online Drivers**

---

## ⚡ Real-Time Communication

RideShere uses SignalR to provide real-time communication between the backend and connected clients.

SignalR is used for:
- Real-time ride requests
- Driver offers
- Trip status updates
- Notifications
- Live communication between passengers and drivers

---

## 🏗️ System Architecture

The project follows a Clean Architecture-inspired layered structure:

**Presentation → Application → Infrastructure → Database**

---

## 🗄️ Database Design

The database handles entities for Users, Drivers, Vehicles, Rides, Offers, Payments, and Reviews, structured to support the real-time matching and verification workflows efficiently.
