
$(document).ready(function () {
    //var formData = new FormData();
    var dtlist = [];
    var batchUpdId = "";
    var myUploadedFile;
    //************* Please Note: This Code is working perfectly fine outside this environment  ************************//
    
    $('#BulkUploadBtn').click(function () {
        $("#BulkUploadBtn").click();
    });

    //$('#BulkUploadInput').change(function () {
    //    var input = document.getElementById("BulkUploadInput");
    //    var files = input.files;
    //    console.log("input: ", input);
    //    console.log("files: ", files);
    //    console.log("loopedFile", files[0]);
    //    myUploadedFile = files[0];
    //    //formData.append(files[0]);
    //    //console.log("formData:", formData);
    //    console.log("myUploadedFile:", myUploadedFile);
    //    for (var i = 0; i != files.length; i++) {
    //        formData.append("file", files[i]);
    //        console.log("loopedFile", files[i]);
    //        console.log("formData:", formData);
    //    }
    //    //$('#selected_filename').text($('#fileinput')[0].files[0].name);
    //    //$('BAUfile').value = $('#BAUfileinput')[0].files[0].name;
    //});

    //document.getElementById("BulkUploadInput").onchange = function (e) {
    //    var file = document.getElementById("BulkUploadInput").files[0];
    //    var reader = new FileReader();
    //    reader.onload = function () {
    //        //console.log(reader.result);
    //        myUploadedFile = reader.result;
    //        //document.getElementById("display").src = reader.result;
    //        // image editing
    //        // ...
    //        //var blob = window.dataURLtoBlob(reader.result);
    //        //formData.append(blob, new File([blob], "image.png", {
    //        //    type: "image/png"
    //        //}));
    //        //console.log(blob, new File([blob], "image.png", {
    //        //    type: "image/png"
    //        //}));
    //    };
    //    reader.readAsDataURL(file);
    //};


    
    //******************  Reviewers Discretion is advised  ************************//

    $('#filterBulkList').change(function () {
        var input = document.getElementById("filterBulkList");
        var filteredList = dtlist.filter(function (bulklist) {
            return bulklist.franchise == input;
        });
        $('#BAUploadTable tbody').empty();
        datalist = filteredList;
        for (var i = 0; i < datalist.length; ++i) {
            //var sn = i + 1;
            $('#BAUploadTable tbody').append('<tr class="btnSelect"><td>' + datalist[i].CUSTOMER_NO + '</td><td>' + datalist[i].CUSTOMER_TYPE + '</td><td>' + datalist[i].CUSTOMER_NAME + '</td><td>' + datalist[i].CUSTOMER_CATEGORY + '</td><td>' + datalist[i].SEX + '</td><td>' + datalist[i].NATIONAL_ID + '</td><td>' + datalist[i].E_MAIL + '</td><td>' + datalist[i].PASSPORT_NO + '</td><td>' + datalist[i].NATIONALITY + '</td><td>' + datalist[i].ACCOUNT_CLASS + '</td></tr>');
        }
    });

    $("#UploadBulkRequest").click(function (evt) {
        evt.preventDefault();
        var file = document.getElementById("BulkUploadInput").files[0];
        var formData = new FormData();
        formData.append('file', file);
        var model = { file: formData};
        $('#BAUploadTable tbody').empty();
        $.ajax({
            url: '/CCOInput/BulkAcctUpload/CustomerUpload',
            type: 'POST',
            data: formData,
            async: false,
            cache: false,
            contentType: false,
            enctype: 'multipart/form-data',
            processData: false,
            success: function (uploadDetails) {
                console.log(uploadDetails);
                if (uploadDetails.data != null && uploadDetails.data.bulkResponse.length > 0) {
                    var datalist = uploadDetails.data.bulkResponse;
                    dtlist = datalist;
                    batchUpdId = datalist[0].BATCH_ID;
                    for (var i = 0; i < datalist.length; ++i) {
                        //var sn = i + 1;
                        $('#BAUploadTable tbody').append('<tr class="btnSelect"><td>' + datalist[i].CUSTOMER_NO + '</td><td>' + datalist[i].CUSTOMER_TYPE + '</td><td>' + datalist[i].CUSTOMER_NAME + '</td><td>' + datalist[i].CUSTOMER_CATEGORY + '</td><td>' + datalist[i].SEX + '</td><td>' + datalist[i].NATIONAL_ID + '</td><td>' + datalist[i].E_MAIL + '</td><td>' + datalist[i].PASSPORT_NO + '</td><td>' + datalist[i].NATIONALITY + '</td><td>' + datalist[i].ACCOUNT_CLASS + '</td></tr>');
                    }
                    //Populate validated bulk if available
                    if (uploadDetails.data.bulkValidateResponse != null && uploadDetails.data.bulkValidateResponse.length > 0) {
                        var vdatalist = uploadDetails.data.bulkValidateResponse;
                        $('#ExistingIdTable tbody').empty();
                        $("#existingbatchNumber").val(batchUpdId);
                        for (var i = 0; i < vdatalist.length; ++i) {
                            //var sn = i + 1;
                            $('#ExistingIdTable tbody').append('<tr class="btnSelect"><td>' + vdatalist[i].cust_name + '</td><td>' + vdatalist[i].cust_type + '</td><td>' + vdatalist[i].cust_no + '</td><td>' + vdatalist[i].id_name + '</td><td>' + vdatalist[i].id_no + '</td></tr>');
                        }
                        ShowModal($("#existIDModal"));
                    }
                } else {
                    Swal.fire("Bulk Upload", "No record found.", "warning");
                }
            },
            error: function(error) {
                //await GlobalErrorResponse(error);
                console.error("error", error);
                Swal.fire("An error occurred", `Reason for failure: ${error.responseJSON.message}`, "error");
            }
        });
    });

    //$('#UploadBulkRequest').click(function () {
    //    //$("#GetStatementDetailFetch").show();
    //    //console.log("afterclick:", myUploadedFile);
    //    var model = JSON.stringify({
    //        file: myUploadedFile
    //    });

    //    console.log("UploadBulkRequest model : ", model);
    //    $('#BAUploadTable tbody').empty();

    //    PostMethod("/CCOInput/BulkAcctUpload/CustomerUpload", model)
    //        .then(uploadDetails => {
    //            if (uploadDetails.data != null && uploadDetails.data.bulkResponse.length > 0) {
    //                var datalist = uploadDetails.data.bulkResponse;
    //                dtlist = datalist;
    //                batchUpdId = datalist[0].BATCH_ID;
    //                for (var i = 0; i < datalist.length; ++i) {
    //                    //var sn = i + 1;
    //                    $('#BAUploadTable tbody').append('<tr class="btnSelect"><td>' + datalist[i].CUSTOMER_NO + '</td><td>' + datalist[i].CUSTOMER_TYPE + '</td><td>' + datalist[i].CUSTOMER_NAME + '</td><td>' + datalist[i].CUSTOMER_CATEGORY + '</td><td>' + datalist[i].SEX + '</td><td>' + datalist[i].NATIONAL_ID + '</td><td>' + datalist[i].E_MAIL + '</td><td>' + datalist[i].PASSPORT_NO + '</td><td>' + datalist[i].NATIONALITY + '</td><td>' + datalist[i].ACCOUNT_CLASS + '</td></tr>');
    //                }
    //                //Populate validated bulk if available
    //                if (uploadDetails.data.bulkValidateResponse != null && uploadDetails.data.bulkValidateResponse.length > 0) {
    //                    var vdatalist = uploadDetails.data.bulkValidateResponse;
    //                    $('#ExistingIdTable tbody').empty();
    //                    $("#existingbatchNumber").val(batchUpdId);
    //                    for (var i = 0; i < vdatalist.length; ++i) {
    //                        //var sn = i + 1;
    //                        $('#ExistingIdTable tbody').append('<tr class="btnSelect"><td>' + vdatalist[i].cust_name + '</td><td>' + vdatalist[i].cust_type + '</td><td>' + vdatalist[i].cust_no + '</td><td>' + vdatalist[i].id_name + '</td><td>' + vdatalist[i].id_no + '</td></tr>');
    //                    }
    //                    ShowModal($("#existIDModal"));
    //                }
    //            } else {
    //                Swal.fire("Bulk Upload", "No record found.", "warning");
    //            }
    //            //$("#GetStatementDetailFetch").hide();
    //        })
    //        .catch(async (error) => {
    //            //$("#GetStatementDetailFetch").hide();
    //            await GlobalErrorResponse(error);
    //        });
    //});

    $('#ProcessBulkBtn').click(function () {
        //$("#GetStatementDetailFetch").show();
        baCreationFor = "";
        if (document.getElementById('newcust').checked) {
            baCreationFor = "newcustomer";
        }
        if (document.getElementById('existcust').checked) {
            baCreationFor = "existingcustomer";
        }
        var model = JSON.stringify({
            batchId: batchUpdId,
            batchtype: baCreationFor
        });
        $('#BAUploadTable tbody').empty();

        PostMethod("/CCOInput/BulkAcctUpload/ProcessBulkRequest", model)
            .then(processBulk => {
                dtlist = [];
                batchUpdId = "";
                if (processBulk.data != null && processBulk.data.status == "true") {
                    Swal.fire("Bulk Upload", processBulk.data.Message, "success");
                } else {
                    Swal.fire("Bulk Upload", "No record found for this criteria.", "warning");
                }
                //$("#GetStatementDetailFetch").hide();
            })
            .catch(async (error) => {
                //$("#GetStatementDetailFetch").hide();
                await GlobalErrorResponse(error);
            });
    });
});
