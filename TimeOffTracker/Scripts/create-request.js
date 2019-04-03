var form = document.getElementById('verificationRequestForm');
var submitBtn = document.getElementById('submitBtn');
var addApproverBtn = document.getElementById('addApproverBtn');
var from = document.getElementById('VacationRequest_DateStart');
var to = document.getElementById('VacationRequest_DateEnd');

form.addEventListener('input', checkFormValidity);

submitBtn.addEventListener('click', () => {
    if (!form.checkValidity()) {
        return;
    }
    var requestData = createRequestData(form.id);
    console.log(requestData);

    $.ajax({
        url: '/VacationRequest/CreateVacationRequest',
        dataType: "json",
        contentType: "application/json",
        method: "POST",
        data: JSON.stringify(requestData),
        success: function (responce) {
            console.log(responce);
        }
    });
});

addApproverBtn.addEventListener('click', addApprover);

function createRequestData(id) {
    id = '#' + id;
    return $(id).serializeArray().reduce(function(obj, item) {
        var part = obj;
        var parts = item.name.split('.');
        if (item.name === 'Approvers') {
            if(!obj[item.name]) {
                obj[item.name] = [];
            }
            obj[item.name].push(JSON.parse(item.value));
            return obj;
        }
        if (item.name === 'VacationRequest.VacationTypes') {
            item.value = JSON.parse(item.value);;
        }
        
        for (let i = 0; i < parts.length - 1; i++) {
            if (!part[parts[i]]) {
                part[parts[i]] = {};
            }
            part = obj[parts[i]];
        }
        part[parts[parts.length - 1]] = item.value;
        return obj;
    }, {});
}
function checkFormValidity() {
    form.checkValidity() ? submitBtn.disabled = false : submitBtn.disabled = true;
}
function addApprover() {
    var approvers = document.getElementsByClassName('approvers');
    var newApprover = approvers[approvers.length - 1].cloneNode(true);
    approvers[approvers.length - 1].append(newApprover);
}
from.addEventListener('change', () => {
   to.setAttribute('min', from.value); 
});
to.addEventListener('change', () => {
   from.setAttribute('max', to.value); 
});

