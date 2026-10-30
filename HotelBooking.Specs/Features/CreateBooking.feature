Feature: Create booking
  As a customer
  I want to book a room for a period
  So that I have a room for my whole stay

  The examples are the black-box test cases from step 1
  (decision table TC1-TC10 and the invalid ECT/BVT cases V1-V6).
  Dates are written as days from today, e.g. day 1 is tomorrow.

  Background:
    Given the hotel has the following rooms
      | Id | Description |
      | 1  | A           |
      | 2  | B           |
    And all rooms are booked from day 10 to day 20

  Scenario Outline: A booking is only created when a room is free for the whole period
    When I book a room from day <Start> to day <End>
    Then the booking is <Result>

    Examples: Decision table (B = before, O = occupied, A = after)
      | Case | SD | ED | Start | End | Result   |
      | TC1  | B  | B  | 1     | 9   | created  |
      | TC2  | A  | A  | 21    | 21  | created  |
      | TC3  | B  | A  | 9     | 21  | rejected |
      | TC4  | B  | O  | 9     | 10  | rejected |
      | TC5  | B  | O  | 9     | 20  | rejected |
      | TC6  | O  | A  | 10    | 21  | rejected |
      | TC7  | O  | A  | 20    | 21  | rejected |
      | TC8  | O  | O  | 10    | 10  | rejected |
      | TC9  | O  | O  | 10    | 20  | rejected |
      | TC10 | O  | O  | 20    | 20  | rejected |

  Scenario Outline: A booking with invalid dates is refused
    When I book a room from day <Start> to day <End>
    Then the booking fails because the dates are invalid

    Examples: Invalid equivalence classes and boundaries
      | Case | Description                   | Start | End |
      | V1   | start date is today           | 0     | 5   |
      | V2   | start date is yesterday       | -1    | 5   |
      | V3   | whole period is in the past   | -5    | -1  |
      | V5   | start date is after end date  | 6     | 5   |
      | V6   | start after O, end before O   | 21    | 9   |
