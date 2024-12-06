let selectedApps = []; // To store selected applications



failed = function (xhr) {
    console.log(xhr)
    alert(`Status: ${xhr.status}, Status Text: ${xhr.statusText}`);
    //generateJSON('outlook')
};

failedSharepoint = function (xhr) {
    console.log(xhr)
    alert(`Status: ${xhr.status}, Status Text: ${xhr.statusText}`);
    //generateJSON('sharepoint')
};

success = function (response) {
    console.log(response)

    if (response.redirect) {

        alert("You need to log in to proceed further. You will be redirected to the Microsoft login page.")

        window.location.href = window.location.origin + response.redirect;
    }
    else {

        alert(response.message);
        generateJSON('outlook')

    }

   
};

successSharepoint = function (response) {
    console.log(response)
    if (response.redirect) {

        alert("You need to log in to proceed further. You will be redirected to the Microsoft login page.")

        window.location.href = window.location.origin + response.redirect;
    }
    else {

        alert(response.message);
        generateJSON('sharePoint')

    }
};


const initializeQuill = (selector, options) => {

    const quill = new Quill(selector, options);

    const resetForm = () => {
        if (options.initialData) {
            quill.setContents(options.initialData);
        }
    };

    resetForm();

    const form = document.querySelector('#mailForm');
    if (form) {
        form.onsubmit = function () {
            document.getElementById('quillContent').value = quill.root.innerHTML;
        };
    }

    return quill;
};

const initialData = {
    about: [
        {
            insert:
                '',
        },
    ],
};

var username = null;
var useremail = null;

var quill = null;

// Function to toggle selection of applications
function toggleSelection(cell, appName) {
    const appIndex = selectedApps.indexOf(appName);
    var slack_token = document.getElementById("slackOAuth_tok").value;
    //alert("slack token:" + slack_token);
    if (appIndex === -1) {
        // Add app to selected list and highlight the cell
        selectedApps.push(appName);
        if (appName == "Slack" && slack_token == "")
        {
           
            slack_Login();
            
        }
        if (appName == "Teams") {
            Teams_Login();
        }
        cell.classList.add('highlighted'); // Add 'highlighted' class to the table cell
        console.log('Added highlighted:', appName); // Debugging log
    } else {
        // Remove app from selected list and remove highlight
        selectedApps.splice(appIndex, 1);
        cell.classList.remove('highlighted'); // Remove 'highlighted' class from the table cell
        console.log('Removed highlighted:', appName); // Debugging log
    }

    // Update the hidden input field
    document.getElementById('selectedApps').value = selectedApps.join(',');
}


// Validate the form and show highlighted applications in an alert
function validateForm(mode) {
    const startDate = document.getElementById('startDate').value;
    const endDate = document.getElementById('endDate').value;
    const message = document.getElementById('message').value;

    if (mode == 'recent') {

        // Get all elements with the 'highlighted' class
        const highlightedElements = document.querySelectorAll('.highlighted');

        // Iterate over the highlighted elements and extract the application name
        highlightedElements.forEach((element) => {
            // Find the image inside the <th> and get its alt text (which is the app name)
            const appName = element.querySelector('img').alt;

            // Add the app name to the selectedApps array
            selectedApps.push(appName);
        });

        // Update the hidden input field
        document.getElementById('selectedApps').value = selectedApps.join(',');

    }

    if (!startDate || !endDate) {
        alert('Please select both Start and End Date & Time.');
        return false;
    }

    if (message === "") {
        alert('Please Add the message.');
        return false;
    }

    if (selectedApps.length == 0) {
        alert("No applications selected.");
        return false;
    }

    return true; // Allow form submission
}
////// Reset functionality for the Cancel button
////document.addEventListener('DOMContentLoaded', function () {
////    const cancelButton = document.querySelector('.btn-secondary');
////    console.log(cancelButton); // Check if the button is found
////    if (cancelButton) {
////        cancelButton.addEventListener('click', function () {
////            // Reset the form fields (start and end dates, and message)
////            document.getElementById('vacationForm').reset();

////            // Clear the message field manually since reset() does not affect it
////            document.getElementById('message').value = '';

////            // Deselect all application icons
////            const appCells = document.querySelectorAll('.appTable'); // All app cells
////            appCells.forEach(cell => {
////                cell.classList.remove('highlighted'); // Remove the 'highlighted' class
////            });

////            // Clear the selectedApps array
////            selectedApps = [];

////            // Optionally clear the error messages if any are displayed
////            document.getElementById('startDateError').style.display = 'none';
////            document.getElementById('endDateError').style.display = 'none';
////        });
////    }
////    // Make sure this only runs once the DOM is fully loaded
////    //document.getElementById('startDate').addEventListener('change', getAndSetDates);
////    //document.getElementById('endDate').addEventListener('change', getAndSetDates);

////    // Optionally, set the default date values on page load if already present
////    //getAndSetDates();
////    username = document.getElementById('username') ==null?"":document.getElementById('username').value;
////    useremail = document.getElementById('emailid') ==null?"":document.getElementById('emailid').value;


  
   
////});
//$(document).on("ajaxComplete", function () {
//    getAndSetDates(); // This triggers when an AJAX request completes
//});
function setPreview(appName) {
    var previewDesc = document.getElementById("previewDesc");
    var previewhtml = document.getElementById("previewhtml");

    document.getElementById("app").style.display = "none";

    document.getElementById("previewDesc").style.display = "block";
    document.getElementById("previewhtml").style.display = "block";


    switch (appName) {
        case 'slack':
            // Make an AJAX call to load the slack view content
            $.ajax({
                url: '/PreviewHtml/slackPreview',  // URL to the slackView action in the controller
                type: 'GET',
                success: function (data) {
                    // Insert the returned data (the partial view content) into the desired element
                    previewhtml.innerHTML = data;
                    // Update the profile section (Presence)
                    setProfile('slack');
                },
                error: function () {

                    alert('Error loading Slack view');
                }
            });
            break;
        case 'teams':
            // Make an AJAX call to load the slack view content
            $.ajax({
                url: '/PreviewHtml/teamsPreview',  // URL to the slackView action in the controller
                type: 'GET',
                success: function (data) {
                    // Insert the returned data (the partial view content) into the desired element
                    previewhtml.innerHTML = data;
                    // Update the profile section (Presence)
                    setProfile('teams');
                },
                error: function () {
                    alert('Error loading Slack view');
                }
            });
            break;
        case 'zoho':
            // Make an AJAX call to load the slack view content
            $.ajax({
                url: '/PreviewHtml/zohoPreview',  // URL to the slackView action in the controller
                type: 'GET',
                success: function (data) {
                    // Insert the returned data (the partial view content) into the desired element
                    previewhtml.innerHTML = data;
                    // Update the profile section (Presence)
                    setProfile('zoho');
                },
                error: function () {
                    alert('Error loading Slack view');
                }
            });
            break;
        case 'outlook':

            const startDateInputMail = document.getElementById("startDate").value;
            const endDateInputMail = document.getElementById("endDate").value;
            const messageMail = null;

            // Construct the query parameters
            const queryParams = `?startDate=${encodeURIComponent(startDateInputMail)}&endDate=${encodeURIComponent(endDateInputMail)}&message=${encodeURIComponent(messageMail)}`;


            $.ajax({
                url: '/PreviewHtml/outlookPreview' + queryParams,  // URL to the slackView action in the controller
                type: 'GET',
                success: function (data) {
                    // Insert the returned data (the partial view content) into the desired element
                    previewhtml.innerHTML = data;
                    // Update the profile section (Presence)
                    setProfile('outlook');
                },
                error: function () {
                    alert('Error loading Slack view');
                },
                complete: function () {
                    console.log("Request completed.");


                    // Initialize Quill editor
                    var previewquill = initializeQuill('#outlookautoReplyMessagePreview', {
                        modules: {
                            toolbar: [
                                ['bold', 'italic'],
                                ['link', 'blockquote', 'code-block', 'image'],
                                [{ list: 'ordered' }, { list: 'bullet' }],
                            ],
                        },
                        theme: 'snow',
                    });
                }
            });
            break;
        case 'sharepoint':
            // Make an AJAX call to load the slack view content
            $.ajax({
                url: '/PreviewHtml/sharepointPreview',  // URL to the slackView action in the controller
                type: 'GET',
                success: function (data) {
                    // Insert the returned data (the partial view content) into the desired element
                    previewhtml.innerHTML = data;
                    // Update the profile section (Presence)
                    setProfile('sharepoint');
                },
                error: function () {
                    alert('Error loading Slack view');
                }
            });
            break;
        default:
            previewDesc.innerHTML = "Your Out of Office message will be displayed here.";
            previewImg.src = "/images/preview_IMGs/placeholder.png";  // Default placeholder
            previewImg.alt = "Preview Image";
            break;
    }
}

function slack_Login() {
    console.log("Get Channels");

    $.ajax({
        url: '/Slack/Slack_singin',  // The controller action URL
        type: 'POST',  // POST method
        success: function (response) {
            // Handle the response from the server
            if (response.redirectUrl) {
                window.location.href = response.redirectUrl; // Redirect the user to the Slack OAuth URL
            }
        },
        error: function (xhr, status, error) {
            // Handle errors
            console.error('Error:', error);
        }
    });
}


function Teams_Login() {
    $.ajax({
        url: '/Home/TeamsLogin',  // The controller action URL
        type: 'POST',  // POST method
        success: function (response) {
            // Handle the response from the server
            if (response.redirectUrl) {
               alert('Success:', "error");// Redirect the user to the Slack OAuth URL
            }
        },
        error: function (xhr, status, error) {
            // Handle errors
            console.error('Error:', error);
        }
    });
}

function selectChannels(selectedChannels = [], appName) {
    const channelList = $('#channelList');

    // Clear any previously selected values
    channelList.val(null);

    // Remove any existing "No channels selected" message if it exists
    const noSelectionMessage = $('#noSelectionMessage');
    if (noSelectionMessage.length) {
        noSelectionMessage.remove();
    }

    // Check if any valid channels are selected
    if (selectedChannels.length > 0 && !selectedChannels.includes("No channels selected")) {
        let isAnyChannelSelected = false;

        if (appName == "slack") {
            // Loop through the selected channels and select them in the dropdown
            channelList.find('option').each(function () {
                const option = $(this);
                if (selectedChannels.includes(option.text())) {
                    option.prop('selected', true); // Select the option
                    isAnyChannelSelected = true;
                }
            });
        }

        if (appName == "teams") {
            // Loop through the selected channels and select them in the dropdown
            channelList.find('option').each(function () {
                const option = $(this);
                const value = option.val(); // Get the option's value
                if (selectedChannels.includes(value)) { // Match using the value
                    option.prop('selected', true); // Select the option
                    isAnyChannelSelected = true;

                    // Log or display the text of the option
                    console.log(option.text()); // Displays "temp"
                }
            });
        }
        // Trigger the 'change' event to process the selection
        channelList.trigger('change');
    } 
}


function setConfig(appName, mode) {
    // 1125 [##] code updated for the new mode config 
    var get;
    let getValueFromDB;
    var selectedChannel;
    if (appName != "sharepoint") {
        try {
            get = getRecentValues();
            getValueFromDB = get.find(item => item.Application.toLowerCase() === appName);
            selectedChannel = getValueFromDB.Channel;
            //if ((appName == "teams" || appName == "slack") && mode == "new") {
            //    // Assuming getValueFromDB is already defined
            //    const { Application, Channel } = getValueFromDB;

            //    // Create a new object with only Application and Channel
            //    getValueFromDB = { Application, Channel };
            //}
            if (mode == "new") {
                getValueFromDB = "";
            }
        }
        catch (Ex) {
            console.log("getrecent value is not available in new mode setting default empty values with new config");
            get = [];
        }
        // 1125 [##] code -Ends
    }

    var appElement = document.getElementById("app");

    // 1125 [##] Code updated for the new mode config of Zoho
    var startDateInput = document.getElementById("startDate").value;
    var endDateInput = document.getElementById("endDate").value;
    var messageInput = document.getElementById("message").value;

    if (getValueFromDB) {
        startDateInput = getValueFromDB.Publish_Schedule?.From_Date || ""; // Assigning value safely
        endDateInput = getValueFromDB.Publish_Schedule?.To_Date || "";    // Assigning value safely
        messageInput = getValueFromDB.Message || "";                     // Assigning value safely
    } else {
        startDateInput = document.getElementById("startDate").value;
        endDateInput = document.getElementById("endDate").value;
        messageInput = document.getElementById("message").value;
    }


    appElement.style.display = "block";

    // 1125 [##] code updated for the new mode config 
    if (document.getElementById("previewDesc") != null) document.getElementById("previewDesc").style.display = "none";
    if (document.getElementById("previewImg") != null) document.getElementById("previewImg").style.display = "none";
    if (document.getElementById("previewhtml") != null) document.getElementById("previewhtml").style.display = "none";
    // 1125 [##] code ends
    switch (appName) {
        case 'slack':
            // Make an AJAX call to load the slack view content


            $.ajax({
                url: '/PartialView/slackView',  // URL to the slackView action in the controller
                type: 'GET',
                success: function (data) {
                    // Insert the returned data (the partial view content) into the desired element
                    appElement.innerHTML = data;
                    getAndSetDates(getValueFromDB, appName);
                    initializeChannelSelection();
                    selectChannels(selectedChannel, appName);
                },
                error: function () {
                    alert('Error loading Slack view');
                }
            });
            break;
        case 'teams':
            // Make an AJAX call to load the teams view content
            $.ajax({
                url: '/PartialView/teamsView',  // URL to the teamsView action in the controller
                type: 'GET',
                success: function (data) {
                    // Insert the returned data (the partial view content) into the desired element
                    appElement.innerHTML = data;
                    getAndSetDates(getValueFromDB, appName);
                    initializeChannelSelection();
                    selectChannels(selectedChannel, appName);
                },
                error: function () {
                    alert('Error loading teams view');
                }
            });




            break;
        case 'zoho':

            if (getValueFromDB == null) { // null means set UI values
                getValueFromDB = {
                    Application: "Zoho",
                    Channel: [],
                    Status: "Scheduled",
                    Message: messageInput,                   
                    Publish_Schedule: { From_Date: startDateInput, To_Date: endDateInput },
                    Reason: "Leave"
                };
            }
            // 1125 [##] code ends

            // Make an AJAX call to load the zoho view content
            $.ajax({
                url: '/PartialView/zohoView',  // URL to the zohoView action in the controller
                type: 'GET',
                success: function (data) {
                    // Insert the returned data (the partial view content) into the desired element
                    appElement.innerHTML = data;
                    getAndSetDates(getValueFromDB, appName);
                },
                error: function () {
                    alert('Error loading zoho view');
                }
            });
            // Trigger the function when Zoho partial view is loaded via AJAX

            break;
        case 'outlook':
            // Make an AJAX call to load the outlook view content

            // Construct the query parameters
            const queryParams = `?startDate=${encodeURIComponent(startDateInput)}&endDate=${encodeURIComponent(endDateInput)}&message=${encodeURIComponent(messageInput)}`;

            $.ajax({
                url: '/global/mail' + queryParams,  // URL to the outlookView action in the controller
                type: 'GET',
                success: function (data) {
                    // Insert the returned data (the partial view content) into the desired element
                    appElement.innerHTML = data;
                    //getAndSetDates();
                },
                error: function () {
                    alert('Error loading outlook view');
                },
                complete: function () {
                    console.log("Request completed.");

                    // Initialize Quill editor
                    quill = initializeQuill('#outlookautoReplyMessage', {
                        modules: {
                            toolbar: [
                                ['bold', 'italic'],
                                ['link', 'blockquote', 'code-block', 'image'],
                                [{ list: 'ordered' }, { list: 'bullet' }],
                            ],
                        },
                        theme: 'snow',
                    });

                    // Handle message content for Quill
                    if (document.getElementById("quillContent")) {

                        document.getElementById("quillContent").value = messageInput;

                        const initialData = {
                            about: [
                                {
                                    insert: messageInput,
                                },
                            ],
                        };

                        quill.setContents(initialData.about);
                    } else {
                        console.warn("Element with ID 'quillContent' not found.");
                    }
                }
            });
            break;
        case 'sharepoint':

            // Construct the query parameters
            const queryParamsShare = `?startDate=${encodeURIComponent(startDateInput)}&endDate=${encodeURIComponent(endDateInput)}&message=${encodeURIComponent(messageInput)}`;


            // Make an AJAX call to load the sharepoint view content
            $.ajax({
                url: '/exchange/getsites' + queryParamsShare,  // URL to the sharepointView action in the controller
                type: 'GET',
                success: function (data) {
                    // Insert the returned data (the partial view content) into the desired element

                    if (data.redirect) {

                        alert("You need to log in to proceed further. You will be redirected to the Microsoft login page.")

                        window.location.href = window.location.origin + data.redirect;
                    }

                    appElement.innerHTML = data;
                    //getAndSetDates(getValueFromDB, appName);

                    //initializeChannelSelection();
                }
                //error: function () {
                //    alert('Error loading outlook view');
                //}
            });
            break;
        default:
            appElement.innerHTML = `<p>App configuration not found.</p>`;
            break;
    }

}
// If views are dynamically loaded, make sure getAndSetDates is called after loading each view

function getAndSetDates(values, appName) {
    // Get references to the input fields
    const startDateInput = document.getElementById("startDate").value;
    const endDateInput = document.getElementById("endDate").value;
    const message = document.getElementById("message").value;

    // Check if inputs exist in the DOM
    if (!startDateInput || !endDateInput) {
        //console.error("Start or end date input fields not found in the DOM.");
        return;
    }

    if (appName == "slack") {
        // Set the values for other fields (e.g., Zoho)
        const slackPresence = document.getElementById("slackPresence");
        const slacksetStatus = document.getElementById("slacksetStatus");
        const slackexpireDuration = document.getElementById("slackexpireDuration");

        if (values) {

            if (values.SlackPresence != null) {
                // Convert values.SlackPresence to lowercase to match option values
                slackPresence.value = values.SlackPresence.toLowerCase();
            }
            if (values.Status != null) {
                slacksetStatus.value = values.Status;
            }
            if (values.SlackExpireDuration != null) {
                slackexpireDuration.value = values.SlackExpireDuration;
            }

        }
    }

    if (appName == "teams") {
        // Set the values for other fields (e.g., Zoho)
        const teamsPresence = document.getElementById("teamsPresence");
        const statusMessage = document.getElementById("statusMessage");
        const expireDuration = document.getElementById("expireDuration");

        if (values) {

            if (values.TeamsStatusMessage != null) {
                statusMessage.value = values.TeamsStatusMessage;
            }
            if (values.Status != null) {
                teamsPresence.value = values.Status;
            }
            if (values.TeamsExpireDuration != null) {
                expireDuration.value = values.TeamsExpireDuration;
            }

        }
    }


    if (appName == "zoho") {

        // Set the values for other fields (e.g., Zoho)
        const zohoStartDate = document.getElementById("zohostartDate");
        const zohoEndDate = document.getElementById("zohoendDate");
        const zohoMessage = document.getElementById("zohoMessage");
        const zohotimeOffType = document.getElementById("zohotimeOffType");
        const zohoreason = document.getElementById("zohoreason");
        const ZohoTimeOffID = document.getElementById("ZohoTimeOffID");

        if (values) {

            zohoStartDate.value = `${values.Publish_Schedule.From_Date}`;
            zohoEndDate.value = `${values.Publish_Schedule.To_Date}`;
            zohoMessage.value = `${values.Message}`;
            ZohoTimeOffID.value = `${values.ZohoTimeOffID}`;

            if (values.TimeOffType != null) {
                zohotimeOffType.value = values.TimeOffType;
            }
            if (values.Reason != null) {
                zohoreason.value = values.Reason;
            }
        }
        else {
            zohoStartDate.value = startDateInput;
            zohoEndDate.value = endDateInput;
            zohoMessage.value = message;
        }
    }
    //if (appName == "sharepoint") {
    //    // Set the values for SharePoint fields
    //    const JanValue = document.getElementById("Jan");
    //    const FebValue = document.getElementById("Feb");
    //    const MarValue = document.getElementById("Mar");
    //    const AprValue = document.getElementById("Apr");
    //    const MayValue = document.getElementById("May");
    //    const JunValue = document.getElementById("Jun");
    //    const JulValue = document.getElementById("Jul");
    //    const AugValue = document.getElementById("Aug");
    //    const SepValue = document.getElementById("Sep");
    //    const OctValue = document.getElementById("Oct");
    //    const NovValue = document.getElementById("Nov");
    //    const DceValue = document.getElementById("Dce");

    //    if (values) {
    //        if (values.Jan != null) {
    //            JanValue.value = values.Jan;
    //        }
    //        if (values.Feb != null) {
    //            FebValue.value = values.Feb;
    //        }
    //        if (values.Mar != null) {
    //            MarValue.value = values.Mar;
    //        }
    //        if (values.Apr != null) {
    //            AprValue.value = values.Apr;
    //        }
    //        if (values.May != null) {
    //            MayValue.value = values.May;
    //        }
    //        if (values.Jun != null) {
    //            JunValue.value = values.Jun;
    //        }
    //        if (values.Jul != null) {
    //            JulValue.value = values.Jul;
    //        }
    //        if (values.Aug != null) {
    //            AugValue.value = values.Aug;
    //        }
    //        if (values.Sep != null) {
    //            SepValue.value = values.Sep;
    //        }
    //        if (values.Oct != null) {
    //            OctValue.value = values.Oct;
    //        }
    //        if (values.Nov != null) {
    //            NovValue.value = values.Nov;
    //        }
    //        if (values.Dce != null) {
    //            DceValue.value = values.Dce;
    //        }
    //    }
    //}

    // Set the values for other fields (e.g., Outlook)
    const outlookStartDate = document.getElementById("outlookstartTime");
    const outlookEndDate = document.getElementById("outlookendTime");

    if (outlookStartDate && outlookEndDate) {
        // Outlook expects full datetime-local format
        outlookStartDate.value = defaultStartDate;
        outlookEndDate.value = defaultEndDate;
        console.log("Outlook dates set successfully.");
    }
}

// Ensure the script runs after the DOM is fully loaded
//document.addEventListener("DOMContentLoaded", getAndSetDates);

function initializeChannelSelection() {
    // Add selected channels to the table
    $('#channelList').on('change', function () {
        const selectedChannelKeys = $(this).val(); // Get the keys of the selected options
        const tableBody = document.getElementById('channelTableBody');

        // Remove the default message row if it exists
        const defaultMessageRow = document.getElementById('defaultMessageRow');
        if (defaultMessageRow) {
            defaultMessageRow.remove();
        }

        selectedChannelKeys.forEach(function (key) {
            // Get the corresponding channel name from the dropdown
            const channelName = $(`#channelList option[value="${key}"]`).text();

            // Check if the channel is already in the table
            if (!$(`#channelTableBody tr[data-key="${key}"]`).length) {
                const newRow = document.createElement('tr');
                newRow.setAttribute('data-key', key); // Store the key in 'data-key'

                const channelCell = document.createElement('td');
                channelCell.textContent = channelName;

                const deleteCell = document.createElement('td');
                const deleteButton = document.createElement('button');
                deleteButton.textContent = 'Delete';
                deleteButton.classList.add('btn', 'btn-danger', 'btn-sm');
                deleteButton.onclick = function () {
                    // Remove the row
                    tableBody.removeChild(newRow);

                    // Show default message if table is empty
                    if (tableBody.children.length === 0) {
                        const defaultRow = document.createElement('tr');
                        defaultRow.id = 'defaultMessageRow';
                        defaultRow.innerHTML = `
                            <td colspan="2" class="text-center">
                                No channels selected.
                            </td>`;
                        tableBody.appendChild(defaultRow);
                    }
                };

                deleteCell.appendChild(deleteButton);
                newRow.appendChild(channelCell);
                newRow.appendChild(deleteCell);
                tableBody.appendChild(newRow);
            }
        });
        // Clear the dropdown selection
        $('#channelList').val(null);
    });

}


let allAppsArr = []; // Global array to store application data

function generateJSON(context) {

    const now = new Date().toLocaleString(); // Current timestamp for Created_Date

    const defaultStartDate = document.getElementById("startDate").value;
    const defaultEndDate = document.getElementById("endDate").value;
    const defaultMessage = document.getElementById("message").value;


    const appData = {
        "slack": () => {
            const slackStatus = document.getElementById("slacksetStatus").value;
            const slackMessage = document.getElementById("message").value;
            const startDate = document.getElementById("startDate").value;
            const endDate = document.getElementById("endDate").value;
            const presence = document.getElementById("slackPresence").value;
            const expiry = document.getElementById("slackexpireDuration").value;
            const selectedChannels = getSelectedChannels();

            return {
                Application: "Slack",
                Channel: selectedChannels,
                Status: slackStatus,
                Message: defaultMessage,
                Publish_Schedule: {
                    From_Date: startDate,
                    To_Date: endDate
                },
                SlackPresence: presence,
                SlackExpireDuration: expiry,
                Created_Date: now
            };
        },
        "zoho": () => {
            const zohoStartDate = document.getElementById("zohostartDate").value;
            const zohoEndDate = document.getElementById("zohoendDate").value;
            const zohoMessage = document.getElementById("zohoMessage").value;
            const zohoTimeOffType = document.getElementById("zohotimeOffType").value;
            const zohoReason = document.getElementById("zohoreason").value;
            const ZohoTimeOffID = document.getElementById("ZohoTimeOffID").value;


            return {
                Application: "Zoho",
                Channel: [], // Example channels; replace with dynamic input if required
                Status: "Scheduled",
                Message: zohoMessage,
                Publish_Schedule: {
                    From_Date: zohoStartDate,
                    To_Date: zohoEndDate
                },
                Created_Date: now,
                TimeOffType: zohoTimeOffType,
                Reason: zohoReason,
                ZohoTimeOffID: ZohoTimeOffID
            };
        },
        "sharePoint": () => {

            //    const JanValue = document.getElementById("Jan").value;
            //    const FebValue = document.getElementById("Feb").value;
            //    const MarValue = document.getElementById("Mar").value;
            //    const AprValue = document.getElementById("Apr").value;
            //    const MayValue = document.getElementById("May").value;
            //    const JunValue = document.getElementById("Jun").value;
            //    const JulValue = document.getElementById("Jul").value;
            //    const AugValue = document.getElementById("Aug").value;
            //    const SepValue = document.getElementById("Sep").value;
            //    const OctValue = document.getElementById("Oct").value;
            //    const NovValue = document.getElementById("Nov").value;
            //    const DceValue = document.getElementById("Dce").value;


            //    return {
            //        Application: "SharePoint",
            //        Channel: [],
            //        Status: "Scheduled",
            //        Message: defaultMessage,
            //        Publish_Schedule: {
            //            From_Date: defaultStartDate,
            //            To_Date: defaultEndDate
            //        },
            //        Created_Date: now,
            //        SharePointColumns: {
            //            Jan: JanValue,
            //            Feb: FebValue,
            //            Mar: MarValue,
            //            Apr: AprValue,
            //            May: MayValue,
            //            Jun: JunValue,
            //            Jul: JulValue,
            //            Aug: AugValue,
            //            Sep: SepValue,
            //            Oct: OctValue,
            //            Nov: NovValue,
            //            Dce: DceValue
            //        }
                //    };

            return {
                Application: "SharePoint",
                Channel: [],
                Status: "Scheduled",
                Message: defaultMessage,
                Publish_Schedule: {
                    From_Date: defaultStartDate,
                    To_Date: defaultEndDate
                },
                Created_Date: now
            };
        },
        "teams": () => {

            const teamsAvailability = document.getElementById("teamsPresence").value; // Availability input
            const teamsStatusMessage = document.getElementById("statusMessage").value; // Custom status message
            const teamsExpireDuration = document.getElementById("expireDuration").value; // Expiry duration for status
            const selectedChannels = getTeamsSelectedChannels(); // Channels selected by the user
            const ChannalMessage = document.getElementById("message").value; // Additional message


            // Map Availability to Activity if necessary
            const availabilityToActivityMap = {
                "Available": "Available",
                "Busy": "Busy", // Default activity for "Busy"
                "Away": "Away",
                "DoNotDisturb": "DoNotDisturb",
                "BeRightBack": "BeRightBack",
                "Offline":"OffWork"
            };

            // Get the mapped activity based on teamsAvailability
            const teamsActivity = availabilityToActivityMap[teamsAvailability] || "Unknown";

            // Construct and return the payload
            return {
                Application: "Teams",
                Status: teamsAvailability, // Renamed from Status to Availability
                Activity: teamsActivity, // Include mapped activity
                TeamsStatusMessage: teamsStatusMessage,
                Message: ChannalMessage,
                Channel: selectedChannels,
                TeamsExpireDuration: teamsExpireDuration,
                Publish_Schedule: {
                    From_Date: defaultStartDate,
                    To_Date: defaultEndDate
                },
                Created_Date: now
            };
        },
        "outlook": () => {

            var startDateElement = document.getElementById("ScheduledStartDateTime-date");
            var endDateElement = document.getElementById("ScheduledEndDateTime-date");
            var messageElement = document.getElementById("quillContent");

            var outLookStartDate = startDateElement ? startDateElement.value : null;
            var outLookEndDate = endDateElement ? endDateElement.value : null;
            var outlookMessage = messageElement ? messageElement.value : null;

            // Extract numeric value from the string
            if (outlookMessage) {
                var tempDiv = document.createElement("div");
                tempDiv.innerHTML = outlookMessage; // Parse as HTML
                outlookMessage = tempDiv.textContent.trim(); // Extract only the text
            }

            if (!outLookStartDate) {
                console.error("Start date is missing or invalid.");
            }

            if (!outLookEndDate) {
                console.error("End date is missing or invalid.");
            }

            if (!outlookMessage) {
                console.error("Message is missing or invalid.");
            }

            if (!outLookStartDate || !outLookEndDate || !outlookMessage) {
                alert("Please ensure all fields are filled correctly.");
            }



            return {
                Application: "Outlook",
                Channel: [],
                Status: "Scheduled",
                Message: outlookMessage,
                Publish_Schedule: {
                    From_Date: outLookStartDate,
                    To_Date: outLookEndDate
                },
                Created_Date: now
            };
        }
    };

    if (appData[context]) {
        // Create or update the application data in the global array
        updateApplication(context, appData[context]());
    }

    console.log(allAppsArr); // Output the final array

    // Convert the array to a JSON string
    const jsonString = JSON.stringify(allAppsArr);

    // Send the stringified JSON to the server
    $.ajax({
        url: "/Home/ReceiveJson",  // Ensure correct URL
        type: "POST",
        contentType: "application/json",
        data: jsonString,  // Pass the stringified JSON
        success: function (response) {
            console.log("Response from server:", response);
            alert("Values stored successfully. Please click 'Publish' to update the database.")
        },
        error: function (xhr, status, error) {
            console.error("Error:", error);
        },
        beforeSend: function (xhr) {
            console.log("Sending request with data:", jsonString);
        }
    });
}

// Utility function to get selected channels from a table
function getSelectedChannels() {
    const tableBody = document.getElementById("channelTableBody");
    const rows = tableBody.getElementsByTagName("tr");
    const selectedChannels = [];
    for (const row of rows) {
        const channelName = row.cells[0].textContent.trim();
        const channelId = row.getAttribute("data-key");

        if (channelId) {
            selectedChannels.push(channelId);
        }
    }
    //return  selectedChannels.join(", "); // Return as a string, separated by commas
    return selectedChannels;
}
function getTeamsSelectedChannels() {
    const tableBody = document.getElementById("channelTableBody");
    const rows = tableBody.getElementsByTagName("tr");
    const selectedChannels = [];
    for (const row of rows) {
        const channelName = row.cells[0].textContent.trim();
        const channelId = row.getAttribute("data-key");

        if (channelId) {
            selectedChannels.push(channelId);
        }
    }
    //return  selectedChannels.join(", "); // Return as a string, separated by commas
    return selectedChannels;
}
// Utility function to update or add an application
function updateApplication(appName, newAppData) {
    const existingIndex = allAppsArr.findIndex(app => app.Application === appName);
    if (existingIndex !== -1) {
        allAppsArr[existingIndex] = newAppData; // Update existing application
    } else {
        allAppsArr.push(newAppData); // Add new application
    }
}
function handleDateChange() {
    const startDate = document.getElementById("startDate").value;
    const endDate = document.getElementById("endDate").value;

    // Format startDate if it exists
    let formattedStartDate = '', formattedStartTime = '';
    if (startDate) {
        const dateObj = new Date(startDate);
        formattedStartDate = dateObj.toLocaleDateString('en-US', {
            month: '2-digit', day: '2-digit', year: 'numeric'
        });
        formattedStartTime = dateObj.toLocaleTimeString('en-US', {
            hour: 'numeric', minute: '2-digit', hour12: true
        });
    }

    // Mapping of app to default and custom messages
    const appMessages = {
        slack: "Out Of Office Message will be published to Slack on your Out of Office day.",
        teams: "Out Of Office Message will be published to Teams on your Out of Office day.",
        zoho: "Time Off request will be published to Zoho immediately after you click [Publish].",
        outlook: "Out Of Office Message will be published to Outlook on your Out of Office day.",
        sharepoint: "Out Of Office Message will be published to SharePoint immediately after you click [Publish]."
    };

    // Custom messages for when both dates are provided
    const customMessage = (app) => `
        Out Of Office Message will be published to ${app} on ${formattedStartDate}, at ${formattedStartTime} (your Out of Office day). 
        Click <a href="#" onclick="setConfig('${app}', 'new'); return false;">here</a> to change when this message is published.
    `;

    // Get all app status cells
    document.querySelectorAll(".app-status-txt").forEach(cell => {
        const app = cell.getAttribute('data-app');
        if (startDate && endDate && ['slack', 'teams', 'outlook'].includes(app)) {
            cell.innerHTML = customMessage(app);
        } else if (startDate && endDate && ['zoho', 'sharepoint'].includes(app)) {
            cell.innerHTML = `
                Time Off request will be published to ${app} immediately after you click [Publish]. 
                Click <a href="#" onclick="setConfig('${app}', 'new'); return false;">here</a> to change when this message is published.
            `;
        } else {
            cell.textContent = appMessages[app];
        }
    });
}

// Validate the fields and show the modal if needed
function valueValidate(selectedTab) {
    const fields = ['.newStartDate', '.newEndDate', '.newMessage'];

    // Loop through the fields to check if any value is entered
    for (const field of fields) {
        const fieldElement = document.querySelector(field);

        if ((fieldElement && fieldElement.value) || selectedApps.length > 0) {
            showModal("Discard changes", "Are you sure you want to discard all the changes?", "Discard", selectedTab);
            return false;
        }
    }

    discardUrl(selectedTab);
}

function clickById(selectedTab) {
    document.getElementById(selectedTab).click();
}

// Update the modal content dynamically based on the passed parameters
function updateModalContent(title, bodyContent, buttonText, selectedTab) {
    const modal = document.getElementById('myDynamicModal');
    if (!modal) return;

    const modalTitle = modal.querySelector('.modal-title');
    const modalBody = modal.querySelector('.modal-body p');
    const actionButton = modal.querySelector('.btn-primary');

    if (modalTitle && modalBody && actionButton) {
        modalTitle.innerText = title;
        modalBody.innerText = bodyContent;
        actionButton.innerText = buttonText;

        if (selectedTab == 'recentPublish') {
            actionButton.onclick = function () {
                clickById(selectedTab);
            }

        }
        else {
            actionButton.onclick = function () {
                discardUrl(selectedTab);
            }
        }
    }
}

    function recentPublishValidation(value) {
        showModal("Warning", "Are you sure you want to update all changes? This will replace the current values with the common values. Click 'here' to update each application separately, or the selected app settings will be updated with the common values.", "Update", value);
        return false;
    }

    // Show the modal by updating the content first and then displaying it
    function showModal(title, bodyContent, buttonText, selectedTab) {
        updateModalContent(title, bodyContent, buttonText, selectedTab);

        const modalElement = document.getElementById('myDynamicModal');
        if (modalElement) {
            const modal = new bootstrap.Modal(modalElement);
            modal.show();
        } else {
            console.error("Modal element not found");
        }
    }

    // Redirect to the selectedTab URL
    function discardUrl(selectedTab) {
        // Get the origin (protocol + host) from the current URL
        const origin = window.location.origin;

        // Construct the URL using the origin and append the path
        const url = `${origin}/Home/${selectedTab}`;

    // Redirect to the constructed URL
    window.location.href = url;
}

function setProfile(apptype) {
    switch (apptype) {
        case 'slack':
            // Get selected values from dropdowns
            const slackPresence = document.getElementById('slackPresence')?.value || 'Unknown'; // Default to 'Unknown' if null/undefined
            const slackSetStatus = document.getElementById('slacksetStatus')?.value || '';     // Default to empty string if null/undefined

            // Update the profile section (Presence)
            const profileStatusElement = document.getElementById('profileStatus');
            if (profileStatusElement) {
                // Update profile presence text and color based on slackPresence
                if (slackPresence === 'Active') {
                    profileStatusElement.innerHTML = '<i class="fa fa-circle"></i> Active'; // FontAwesome circle icon for Active
                    profileStatusElement.style.color = 'green';
                } else if (slackPresence === 'Away') {
                    profileStatusElement.innerHTML = '<i class="fa fa-circle"></i> Away'; // FontAwesome circle icon for Away
                    profileStatusElement.style.color = 'orange';
                } else {
                    profileStatusElement.innerHTML = '<i class="fa fa-circle"></i> Unknown'; // FontAwesome circle icon for Unknown
                    profileStatusElement.style.color = 'gray';
                }
            }

            // Update the status section (Status)
            const statusIcon = document.getElementById('statusIcon');
            const statusText = document.getElementById('statusText');
            const previewUsername = document.getElementById('pname');

            if (previewUsername) {
                const username = document.getElementById('username')?.value || 'Guest'; // Default to 'Guest' if null/undefined
                previewUsername.textContent = username;
            }

            if (statusIcon && statusText) {
                switch (slackSetStatus) {
                    case 'inMeeting':
                        statusIcon.innerHTML = '&#x1F4C5;'; // Calendar (In Meeting)
                        statusText.innerText = 'In Meeting';
                        break;
                    case 'commuting':
                        statusIcon.innerHTML = '&#x1F68C;'; // Bus (Commuting)
                        statusText.innerText = 'Commuting';
                        break;
                    case 'outSick':
                        statusIcon.innerHTML = '&#x1F915;'; // Sick face (Out Sick)
                        statusText.innerText = 'Out Sick';
                        break;
                    case 'vacationing':
                        statusIcon.innerHTML = '&#x1F334;'; // Palm tree (Vacationing)
                        statusText.innerText = 'Vacationing';
                        break;
                    case 'workingRemotely':
                        statusIcon.innerHTML = '&#x1F3E0;'; // House (Working Remotely)
                        statusText.innerText = 'Working Remotely';
                        break;
                    default:
                        statusIcon.innerHTML = '🌴'; // Default icon for unknown status
                        statusText.innerText = 'On holiday';
                        break;
                }
            }

            break;
        case 'teams':
            // Step 1: Get all required IDs first
            const teamsPresence = document.getElementById('teamsPresence')?.value || 'Unknown'; // Default to 'Unknown' if null/undefined
            const teamsstatusmsg = document.getElementById('statusMessage')?.value || 'No Status'; // Default to 'No Status' if null/undefined

            // Get elements for updating the profile section
            const profileStatusElementteams = document.getElementById('profileStatus');
            const teamsstatusmsgset = document.getElementById('statesmsg');
            const profileName = document.getElementById('profileName')?.textContent || 'Unknown';
            const profileEmail = document.getElementById('profileEmail')?.textContent || 'No email';
            // Step 2: Set values for profile section
            profileName.textContent = username;
            profileEmail.textContent = useremail;

            if (profileStatusElementteams) {
                // Step 3: Update profile presence text and color based on teamsPresence
                switch (teamsPresence) {
                    case 'Available':
                        profileStatusElementteams.innerHTML = '&#x1F7E2; Available'; // Green circle (Available)
                        profileStatusElementteams.style.color = 'green';
                        break;
                    case 'Busy':
                        profileStatusElementteams.innerHTML = '&#x1F534; Busy'; // Red circle (Busy)
                        profileStatusElementteams.style.color = 'red';
                        break;
                    case 'DoNotDisturb':
                        profileStatusElementteams.innerHTML = '&#x1F6AB; Do Not Disturb'; // No entry sign (Do Not Disturb)
                        profileStatusElementteams.style.color = 'purple';
                        break;
                    case 'Away':
                        profileStatusElementteams.innerHTML = '&#x23F3; Away'; // Hourglass (Away)
                        profileStatusElementteams.style.color = 'orange';
                        break;
                    case 'BeRightBack':
                        profileStatusElementteams.innerHTML = '&#x23F0; Be Right Back'; // Alarm clock (Be Right Back)
                        profileStatusElementteams.style.color = 'blue';
                        break;
                    case 'Offline':
                        profileStatusElementteams.innerHTML = '&#x26AB; Offline'; // Black circle (Offline)
                        profileStatusElementteams.style.color = 'gray';
                        break;
                    default:
                        profileStatusElementteams.innerHTML = '&#x1F7E0; Unknown'; // Orange circle (Unknown)
                        profileStatusElementteams.style.color = 'black';
                        break;
                }
            }

            // Step 4: Set the status message
            if (teamsstatusmsgset) {
                teamsstatusmsgset.value = teamsstatusmsg;
            }

            break;
        case 'zoho':
         
            // Assuming you have the values stored in variables
            var timeOffType = document.getElementById('zohotimeOffType').value || null;
            var reason = document.getElementById('zohoreason').value || null;
            var startDate = document.getElementById('zohostartDate').value || null;
            var endDate = document.getElementById('zohoendDate').value || null;
            var message = document.getElementById('zohoMessage').value || null;
            var datediffernt = calculateDateDifference(startDate, endDate);

            // Set values in the corresponding controls
            document.getElementById('zohoprefix-timeoff-type-select').value = timeOffType || '';  // Set the Time Off Type
            document.getElementById('zohoprefix-reason-select').value = reason || '';  // Set the Reason
            document.getElementById('zohoprefix-start-date-time-input').value = startDate || '';  // Set Start Date
            document.getElementById('zohoprefix-end-date-time-input').value = endDate || '';  // Set End Date
            document.getElementById('zohoprefix-comments-textarea').value = message || '';  // Set Comments (message)

            // Set Days Remaining (use textContent for labels, not value)
            document.getElementById('zohoprefix-days-remaining-label').textContent = datediffernt || '';  // Set Days Remaining

            break;
        case 'outlook':
            document.getElementById("previewtext").style.display = 'none'; // Hide preview text (e.g., "2024-11-01")

            // Get values from the elements by their ID
            var startDateElement = document.getElementById("ScheduledStartDateTime-date").value || null;
            var startTimeElement = document.getElementById("ScheduledStartDateTime-time").value || null;
            var endDateElement = document.getElementById("ScheduledEndDateTime-date").value || null;
            var endTimeElement = document.getElementById("ScheduledEndDateTime-time").value || null;

            //// Convert date and time values to strings and set them
            //document.getElementById('startDate').value = startDateElement.toString(); // Convert start date to string and set
            //document.getElementById('startTime').value = startTimeElement.toString();// Convert start time to string and set
            //document.getElementById('endDate').value = endDateElement.toString();  // Convert end date to string and set
            //document.getElementById('endTime').value = endTimeElement.toString(); // Convert end time to string and set

            //// Convert date and time values to strings and set them
            document.getElementById('outpreviewstartDate').value = startDateElement; // Convert start date to string and set
            document.getElementById('outpreviewstartTime').value = startTimeElement;// Convert start time to string and set
            document.getElementById('outpreviewendDate').value = endDateElement;  // Convert end date to string and set
            document.getElementById('outpreviewendTime').value = endTimeElement; // Convert end time to string and set

            // Handle rich text content from Quill, if it exists
            if (quill && quill.root) {
                const quillContent = quill.root.innerHTML; // Get content from Quill editor
                const outlookPreviewTextarea = document.getElementById('outlookautoReplyMessagePreview');

                if (outlookPreviewTextarea) {
                    outlookPreviewTextarea.innerHTML = quillContent; // Set Quill content
                } else {
                    console.error("outlookautoReplyMessagePreview element is missing");
                }
            } else {
                console.error("Quill editor content is missing");
            }
            break;
        case 'sharepoint':
            // Get the table element
            const table = document.getElementById('sharepointTable');

            // Initialize an empty collection for key-value pairs
            const keyValuePairs = {};

            // Loop through all rows in the table body
            const rows = table.querySelectorAll('tbody tr');
            rows.forEach(row => {
                const monthCell = row.querySelector('td:first-child');
                const textArea = row.querySelector('textarea');

                if (monthCell && textArea) {
                    const month = monthCell.textContent.trim();
                    const value = textArea.value.trim();

                    // Add to the key-value collection
                    keyValuePairs[month] = value;
                }
            });

            // Log the key-value pairs for testing
            console.log(keyValuePairs);

            //set values
            // Get the first textarea (readonly) in the first row (ID column)
            document.querySelector('#ShareoutTable tbody tr:first-child td:first-child textarea').value = useremail;

            // Get the table element by ID
            const sharetable = document.querySelector('#ShareoutTable');

            // Get all rows in the tbody
            const sharerows = sharetable.querySelectorAll('tbody tr');

            // Iterate over each row (assuming one or more rows in the tbody)
            sharerows.forEach(row => {
                const cells = row.querySelectorAll('td');

                // Loop through each cell and fill the corresponding month value
                cells.forEach((cell, index) => {
                    const textArea = cell.querySelector('textarea');
                    if (textArea && index > 0) { // Skip the first column (ID column)
                        const headerCell = sharetable.querySelector(`thead th:nth-child(${index + 1})`);
                        if (headerCell) {
                            const month = headerCell.textContent.trim();
                            // Set the value from the keyValuePairs collection
                            textArea.value = keyValuePairs[month] || ''; // If no value, default to empty string
                        }
                    }
                });
            });



            break;





     
    }
}

function calculateDateDifference(startDate, endDate) {
    // Calculate the difference in milliseconds between the end and start date
    const diffMillis = new Date(endDate) - new Date(startDate);

    // If the difference is negative, it means the end date is earlier than the start date
    // Return a message indicating the date range is invalid
    if (diffMillis < 0) return "Invalid date range";

    // Calculate the number of full days between the two dates
    const diffDays = Math.floor(diffMillis / (1000 * 60 * 60 * 24)); // Days

    // Calculate the remaining minutes after extracting the full days
    const diffMinutes = Math.floor((diffMillis % (1000 * 60 * 60 * 24)) / (1000 * 60)); // Minutes

    // Return the formatted string with the number of days and minutes (if any)
    return `${diffDays} Day(s) ${diffMinutes > 0 ? `${diffMinutes} min` : ''}`;
}

completed = function (xhr) {
    alert(`Hi ${xhr}!`);
};

const isOk = response => response.ok ? response.json() : Promise.reject(new Error('Failed to load data from server'))

function deleteSingleJob(button) {

    const AppName = button.getAttribute('data-job-name');

    const UAIDElement = document.getElementById('getUAID');
    var AppId;

    if (!UAIDElement) {

        document.getElementById('deleteBtn').setAttribute("disable", true)
    }
    else {
        AppId = UAIDElement.value;
    }

    const url = `/jobs/delete/${AppId}?name=${AppName}`;

    const spinnerOverlay = document.getElementById('spinner-overlay');

    if (confirm("Are you sure you want to delete this job?")) {

        removeClass(spinnerOverlay, "loader-hidden");

        fetch(url, {
            method: 'DELETE',
        })
        .then(isOk)
        .then(data => {
            addClass(spinnerOverlay, "loader-hidden");
            alert(`Job(s) deleted successfully`);
            location.reload();
            console.log(data) 
        })
        .catch(error => {
            addClass(spinnerOverlay, "loader-hidden");
            alert(error)
            console.error(error)

        })


    }
}

function addClass(element, className) {
    if (!element.classList.contains(className)) {
        element.classList.add(className);
    }
}

function removeClass(element, className) {
    if (element.classList.contains(className)) {
        element.classList.remove(className);
    }
}

function deleteJob(button) {

    const AppId = button.getAttribute('data-job-id');
    const url = `/jobs/delete-jobs/${AppId}`;
    const spinnerOverlay = document.getElementById('spinner-overlay');

    if (confirm("Are you sure you want to delete this job?")) {

        removeClass(spinnerOverlay, "loader-hidden");

        fetch(url, {
            method: 'DELETE',
        })
        .then(isOk)
        .then(data => {
                addClass(spinnerOverlay, "loader-hidden");
                alert(`Job(s) deleted successfully: ${data}`);
                location.reload();
                console.log(data) // Prints result from `response.json()`
        })
        .catch(error => {
                addClass(spinnerOverlay, "loader-hidden");
                alert(error)
                console.error(error)

        })
    }
}





