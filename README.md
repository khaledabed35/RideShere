# 🚗 RideShere

A real-time ride-sharing backend system built with ASP.NET Core Web API.

---

## 📌 What is RideShere?

RideShere is an Uber-like ride-sharing platform that connects passengers
with nearby online drivers in real time.

The passenger requests a ride by providing:

- 📍 Pickup location
- 📍 Drop-off location
- 💰 Initial estimated fare
- 💳 Payment method

Nearby drivers receive the ride request in real time and can submit their
own fare offers.

The passenger can compare available offers based on:

- Driver name & rating
- Vehicle information
- Proposed fare
- Distance
- Estimated arrival time

The passenger then selects one driver and the trip begins.

---

## 🔄 Complete Ride Flow

<p align="center">
  <img src="docs/images/ride-flow.png" alt="RideShere Ride Flow" width="900"/>
</p>

### Flow

Passenger requests a ride → nearby drivers are discovered → drivers receive
real-time notifications → drivers submit offers → passenger selects an offer
→ trip starts → trip is completed → payment is processed → passenger leaves
a review.

---

## 🚗 Driver Verification

Drivers must be verified before they can receive ride requests.

<p align="center">
  <img src="docs/images/driver-verification.png" alt="Driver Verification Flow" width="700"/>
</p>

The verification process is:

**Registration → Upload Documents → Pending → Admin Review → Approved / Rejected**

Only approved drivers can go online and receive ride requests.

---

## 📍 Geolocation & Nearby Drivers

RideShere uses geographical coordinates to find nearby online drivers.

<p align="center">
  <img src="docs/images/geolocation-flow.png" alt="RideShere Geolocation Flow" width="700"/>
</p>

Driver location:

**Longitude + Latitude → Redis GEO → Radius Search → Nearby Online Drivers**

---

## ⚡ Real-Time Communication

RideShere uses SignalR to provide real-time communication between the
backend and connected clients.

<p align="center">
  <img src="docs/images/realtime-architecture.png" alt="RideShere Real-Time Architecture" width="900"/>
</p>

SignalR is used for:

- Real-time ride requests
- Driver offers
- Trip status updates
- Notifications
- Live communication between passengers and drivers

---

## 🏗️ System Architecture

<p align="center">
  <img src="docs/images/architecture.png" alt="RideShere Architecture" width="900"/>
</p>

The project follows a Clean Architecture-inspired layered structure:

**Presentation → Application → Infrastructure → Database**

---

## 🗄️ Database Design

<p align="center">
  <img src="# 🚗 RideShere

A real-time ride-sharing backend system built with ASP.NET Core Web API.

---

## 📌 What is RideShere?

RideShere is an Uber-like ride-sharing platform that connects passengers
with nearby online drivers in real time.

The passenger requests a ride by providing:

- 📍 Pickup location
- 📍 Drop-off location
- 💰 Initial estimated fare
- 💳 Payment method

Nearby drivers receive the ride request in real time and can submit their
own fare offers.

The passenger can compare available offers based on:

- Driver name & rating
- Vehicle information
- Proposed fare
- Distance
- Estimated arrival time

The passenger then selects one driver and the trip begins.

---

## 🔄 Complete Ride Flow

<p align="center">
  <img src="docs/images/ride-flow.png" alt="RideShere Ride Flow" width="900"/>
</p>

### Flow

Passenger requests a ride → nearby drivers are discovered → drivers receive
real-time notifications → drivers submit offers → passenger selects an offer
→ trip starts → trip is completed → payment is processed → passenger leaves
a review.

---

## 🚗 Driver Verification

Drivers must be verified before they can receive ride requests.

<p align="center">
  <img src="docs/images/driver-verification.png" alt="Driver Verification Flow" width="700"/>
</p>

The verification process is:

**Registration → Upload Documents → Pending → Admin Review → Approved / Rejected**

Only approved drivers can go online and receive ride requests.

---

## 📍 Geolocation & Nearby Drivers

RideShere uses geographical coordinates to find nearby online drivers.

<p align="center">
  <img src="docs/images/geolocation-flow.png" alt="RideShere Geolocation Flow" width="700"/>
</p>

Driver location:

**Longitude + Latitude → Redis GEO → Radius Search → Nearby Online Drivers**

---

## ⚡ Real-Time Communication

RideShere uses SignalR to provide real-time communication between the
backend and connected clients.

<p align="center">
  <img src="docs/images/realtime-architecture.png" alt="RideShere Real-Time Architecture" width="900"/>
</p>

SignalR is used for:

- Real-time ride requests
- Driver offers
- Trip status updates
- Notifications
- Live communication between passengers and drivers

---

## 🏗️ System Architecture

<p align="center">
  <img src="docs/images/architecture.png" alt="RideShere Architecture" width="900"/>
</p>

The project follows a Clean Architecture-inspired layered structure:

**Presentation → Application → Infrastructure → Database**

---

## 🗄️ Database Design

<p align="center">
  <img src="docs/images/database-erd.png" alt="RideShere Database ERD" width="1000"/>
</p>" alt="RideShere Database ERD" width="1000"/>
</p>
