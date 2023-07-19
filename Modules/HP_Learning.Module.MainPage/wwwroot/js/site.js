let commune_id = 0;
let districts_id = 0;
let _id = 0;
//ui semantic
$('.ui.form').form({
    fields: {
        truong: {
            identifier: 'truong',
            rules: [
                {
                    type: 'empty',
                    prompt: 'Vui lòng nhập Trường!'
                }
            ]
        },
        lop: {
            identifier: 'lop',
            rules: [
                {
                    type: 'empty',
                    prompt: 'Vui lòng nhập Lớp!'
                }
            ]
        },
        ho_ten: {
            identifier: 'ho_ten',
            rules: [
                {
                    type: 'empty',
                    prompt: 'Vui lòng nhập họ tên!'
                }
            ]
        }, district_code: {
            identifier: 'district_code',
            rules: [
                {
                    type: 'empty',
                    prompt: 'Vui lòng chọn Quận/Huyện'
                }
            ]
        }, commune_code: {
            identifier: 'commune_code',
            rules: [
                {
                    type: 'empty',
                    prompt: 'Vui lòng chọn Xã/Phường'
                }
            ]
        },

    }, onSuccess: function (event) {
        event.preventDefault();
        var data = {};
        data.ho_ten = $('input[name="ho_ten"]').val();
        data.truong = $('input[name="truong"]').val();
        data.lop = $('input[name="lop"]').val();
        data.so_dt = $('input[name="so_dt"]').val();
        data.email = $('input[name="email"]').val();
        data.commune_code = commune_id;
        data.district_code = districts_id;
        let file = uppy.getFiles()[0];
        if (file != null) {
            let fileFormData = new FormData();
            fileFormData.append("chunkContent", file.data);
            $.ajax({
                type: "post",
                url: "/_upload/documents",
                data: fileFormData,
                cache: false,
                async: false,
                dataType: 'json',
                processData: false,
                contentType: false,
                success: (xhr) => {

                }, error(e) {
                    console.log(e)
                }
            }).done(xhr => {
                if (xhr.status === "OK") {
                    file.url = "/_documents/" + xhr.data;
                    var FileDK = {};
                    FileDK.ten_dinhkem = file.name;
                    $("#drag-drop-area").css("display", "none")
                    FileDK.duong_dan = file.url;
                    let token = $('input[name="__RequestVerificationToken"]').val();
                    $.ajax({
                        type: 'post',
                        url: '/upload',
                        data: { __RequestVerificationToken: token, bdt: data, dk: FileDK },
                        success: xhr => {
                            if (xhr.status == "OK") {
                                let uid = xhr.data.uid;
                                $('.ui.form').form('reset');
                                $("#drag-drop-area").css("display", "block")
                                let id = uppy.getFiles()[0].id;//reset File
                                uppy.removeFile(id);//reset File
                                $("#ui_form").find(".text-error").empty().css('background-color', '#fff');//reset validate file
                                window.open('/review/' + uid, "_self");
                            } else {
                                $('#uploadFail').modal('show');
                                setTimeout(function () { $('#uploadFail').modal('hide') }, 2500);
                            }
                        },
                        error: _ => {
                            $('#errorUpload').modal('show');
                            setTimeout(function () { $('#errorUpload').modal('hide') }, 2500);
                        }
                    });
                } else {
                    $('#errorUpload').modal('show');
                    setTimeout(function () { $('#errorUpload').modal('hide') }, 2500);
                }
            })
        }
        else {
            $('#loader').modal('hide');
            $('#loader').removeClass('active');
            $("#ui_form").find(".text-error").text("Vui lòng chọn file").addClass('text-error').css({ 'background-color': '#999999', 'padding': '6px', 'border-radius': '8px', 'color': '#fff' });
        }
    }
});
$.get('/districts').done(xhr => {
    if (xhr.status == "OK") {
        $('#district_code').append(xhr.data.map(x => `<option value='${x.area_code}'>${x.name_vn}</option>`));
    }
});
//onchange event
$(document).on("change", "#commune_code", function () {
    commune_id = $(this).val()
})
$(document).on("change", "#district_code", function () {
    _id = $('#district_code').val()
    districts_id = $(this).val()
    $.ajax({
        type: 'get',
        url: '/getItemCommune',
        data: { id: _id },
        success: xhr => {
            if (xhr.status == "OK") {
                $('#commune_code').empty().append(xhr.data.map(x => `<option value='${x.area_code}'>${x.name_vn}</option>`));
            }
            commune_id = xhr.data[0].area_code;
        },
        error: _ => {

        }
    });
})
$(document).on("click", "#btn-success", function () {

})
//ẩn uppy dashboard action
$(document).on("click", ".uppy-u-reset", function () {
    $(".uppy-Dashboard-progressindicators").css("display", "none");
})
    // load danh sách bài thi
$(document).ready(function () {
    let g_$table = $('#table');
    let g_oTable = g_$table.DataTable({
        serverSide: true,
        processing: true,
        responsive: true,
        autoWidth: false,
        ordering: false,
        search: false,
        oLanguage: {
            "sSearch": "Tìm kiếm: ",
            "sInfo": "Bản ghi từ _START_ tới _END_ ",
            "sLengthMenu": '<select class="custom-select custom-select-sm form-control form-control-sm">' +
                '<option value="5">5</option>' +
                '<option value="8">8</option>' +
                '<option value="10">10</option>' +
                '<option value="25">25</option>' +
                '<option value="50">50</option>' +
                '<option value="999999999">Tất Cả</option>' +
                '</select>',
            "sInfoFiltered": "(trong tổng _MAX_ bản ghi)",
            "sZeroRecords": "Không tìm thấy danh sách bài thi!",
            "sLoadingRecords": "Đang tải dữ liệu...",
            "sProcessing": "Đang tải dữ liệu...",
            "sInfoEmpty": "Bản ghi từ 0 đến 0",
            "oPaginate": {
                "sNext": "<i class='angle right icon'></i>",
                "sPrevious": "<i class='angle left icon'></i>",
            },
        },
        ajax: {
            url: '/danhSachBaiThi/list',
            type: 'POST',
            data: function (data) {
                data.communes = commune_id;
                data.districts = districts_id;
                return data;
            }
        },
        columns: [
            {
                width: '7%',
                data: 'Id',
                title: 'STT',
                render: function (data, type, row, meta) {
                    return meta.row + meta.settings._iDisplayStart + 1;
                }
            },
            {
                data: 'ho_ten',
                title: 'Họ tên',
            },
            {
                data: 'truong',
                title: 'Trường'
            },
            {
                data: 'lop',
                title: 'Lớp'
            },
            {
                data: null,
                width: '10%',
                title: 'Thao tác',
                className: 'text-center',
                render: function (data, type, full, meta) {
                    return `<div class="btn btn-sm btn-clean btn-icon" title="Xem bài thi"><a href='${data.dinhkems[0].duong_dan}' target="_blank" download="${data.dinhkems[0].duong_dan}" style="font-size: 14px;">Xem bài thi</a></div>`;
                },
            },
        ],
        columnDefs: [

        ],
        dom: "<'row row__mt'<''l><''f>>" +
            "<''<''tr>>" +
            "<'row justify-content-between mx-auto w-100 row__mb'<''i><''p>>",
        initComplete: _ => {
            $('#table_filter label').before(`<select class="form-control select-control" style="width: auto;display: inline-block;margin-right: 10px; margin-bottom: 15px; font-family: 'Comfortaa', sans-serif" id="disTricts">
                                                    <option value="0" selected >Tất cả Quận/Huyện</option>
                                                </select>`);
            $('#table_filter label').before(`<select class="form-control select-control" style="width: auto;display: inline-block;margin-right: 10px;font-family: 'Comfortaa', sans-serif" id="comMunes">
                                                    <option value="0" selected >Tất cả Phường/Xã</option>
                                                </select>`);

            $.get('/districts').done(xhr => {
                if (xhr.status == "OK") {
                    $('#disTricts').append(xhr.data.map(x => `<option value='${x.area_code}'>${x.name_vn}</option>`));
                }
            });
            $(document).on('change', '#disTricts', function (data) {
                _id = $('#disTricts').val()
                districts_id = $(this).val()
                commune_id = 0;
                $.ajax({
                    type: 'get',
                    url: '/getItemCommune',
                    data: { id: _id },
                    success: xhr => {
                        if (xhr.status == "OK") {
                            $('#comMunes').empty().append(`<option value='0'>Tất cả Phường/Xã</option>`);
                            $('#comMunes').append(xhr.data.map(x => `<option value='${x.area_code}'>${x.name_vn}</option>`));
                        } else {
                        }
                        console.log(commune_id)

                    },
                    error: _ => {

                    }
                });

                console.log(districts_id)
                RefreshTable();

            });
            $(document).on('change', '#comMunes', function (data) {
                commune_id = $(this).val()
                console.log(commune_id)

                RefreshTable();
            });

        }
    });
    function RefreshTable() {
        g_oTable.ajax.reload();
    }
});
