        public async Task<Response<UpdateUserResponse>> UpdateUserInfoAsync(UpdateUserRequest model)
        {
            var endpoint = $"OAuth/updateUser";
            var url = _config["oxIdentity:baseUrl"] + endpoint;

            var client = new RestClient(url);
            var request = new RestRequest(Method.POST);

            request.AddParameter("UserCode", model.UserCode);
            request.AddParameter("Email", model.Email);
            request.AddParameter("HouseAddress", model.HouseAddress);
            request.AddParameter("PhoneNumber", model.PhoneNumber);

            try
            {
                if (model.PhotoFile != null)
                    request.AddFile("PhotoFile", model.PhotoFileUrl);

                else
                    request.AddFile("PhotoFile", null);
            }

            catch (Exception ex)
            {
                Log.Error("PhotoFile UPLOAD Error", ex.Message);
                //request.AddFile("PhotoFile", null);
            }

            request.AddParameter("ReferredBy", model.ReferredBy);
            request.AddParameter("FirstName", model.FirstName);
            request.AddParameter("LastName", model.LastName);
            request.AddParameter("MiddleName", model.MiddleName);
            //request.AddParameter("BVN", model.BVN);
            request.AddParameter("Country", model.Country);
            request.AddParameter("DateOfBirth", model.DateOfBirth);
            request.AddParameter("Gender", model.Gender);
            request.AddParameter("StateName", model.StateName);
            request.AddParameter("StreetName", model.StreetName);
            request.AddParameter("LGA", model.LGA);



            Serilog.Log.Information($"UpdateUserInfo Request:: {JsonConvert.SerializeObject(model)}");
            var result = await client.ExecuteAsync(request);
            Serilog.Log.Information($"UpdateUserInfo Response:: {result.Content}");

            var response = JsonConvert.DeserializeObject<Response<UpdateUserResponse>>(result.Content);

            if (response == null)
                return await Task.FromResult(new Response<UpdateUserResponse>
                {
                    Message = "No response from api."
                });

            if (!response.Success)
            {
                return await Task.FromResult(
                    new Response<UpdateUserResponse>
                    {
                        Message = response.Message,
                    });

            }

            return await Task.FromResult(
                       new Response<UpdateUserResponse>
                       {
                           Message = response.Message,
                           Success = true,
                           Data = response.Data
                       });
        }
