Feature: Fully occupied dates
  A date is fully occupied when every room has an active booking on that date.
  Dates are written as days from today.

  Background:
    Given the hotel has the following rooms
      | Id | Description |
      | 1  | A           |
      | 2  | B           |

  Scenario: Only dates where all rooms are booked are fully occupied
    Given the following bookings
      | Room | Start | End | Active |
      | 1    | 10    | 20  | true   |
      | 2    | 15    | 25  | true   |
    When I ask for the fully occupied dates from day 1 to day 30
    Then the fully occupied dates are day 15 to day 20

  Scenario: Inactive bookings do not count
    Given the following bookings
      | Room | Start | End | Active |
      | 1    | 10    | 20  | true   |
      | 2    | 10    | 20  | false  |
    When I ask for the fully occupied dates from day 1 to day 30
    Then there are no fully occupied dates
