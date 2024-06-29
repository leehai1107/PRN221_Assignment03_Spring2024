// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
"use strict";

var connection = new signalR.HubConnectionBuilder()
    .withUrl("/signalRServer")
    .build();

//connection.on("LoadCourses", function () {
//    location.href='/Courses'
//});
//connection.on("LoadInstructors", function () {
//    location.href='/Instructors'
//});
connection.on("LoadNewsArticles", function () {
    location.href = '/NewsArticles'
});

connection.on("ReceiveReportUpdate", function (newsArticles) {
    const tableBody = document.getElementById("reportTableBody");
    tableBody.innerHTML = ""; // Clear the existing rows

    newsArticles.forEach(article => {
        const row = document.createElement("tr");

        const titleCell = document.createElement("td");
        titleCell.textContent = article.title;
        row.appendChild(titleCell);

        const dateCell = document.createElement("td");
        dateCell.textContent = new Date(article.createdDate).toLocaleString();
        row.appendChild(dateCell);

        tableBody.appendChild(row);
    });
});

connection.start().catch(function (err) {
    return console.error(err.toString());
});