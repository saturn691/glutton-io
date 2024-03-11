/*
 * Implements a LP filter according to FIR3 in the virtual FPGA
 *
*/

module fir(
	//////////// INPUTS //////////
	input					clk,
	input signed 	[31:0]	in,
	
	//////////// OUTPUT //////////
	output signed	[31:0]	out
);


//=======================================================
//  REG/WIRE declarations
//=======================================================

	reg signed 	[31:0] memory [42:0];
	
	wire signed [63:0] temp	  [42:0];
	wire signed [63:0] intermediate;
	
	reg signed 	[31:0] coeffs [42:0];
	

//=======================================================
//  Structural coding
//=======================================================
	
	// 43-tap FIR filter
	initial begin
		coeffs[42] = 32'h205bc01;
		coeffs[41] = 32'h2617c1b;
		coeffs[40] = 32'h2ca57a7;
		coeffs[39] = 32'h32ca57a;
		coeffs[38] = 32'h39c0ebe;
		coeffs[37] = 32'h40b7803;
		coeffs[36] = 32'h47ae147;
		coeffs[35] = 32'h4ea4a8c;
		coeffs[34] = 32'h559b3d0;
		coeffs[33] = 32'h5c91d14;
		coeffs[32] = 32'h631f8a0;
		coeffs[31] = 32'h6944673;
		coeffs[30] = 32'h6f69446;
		coeffs[29] = 32'h7525460;
		coeffs[28] = 32'h7a0f909;
		coeffs[27] = 32'h7e90ff9;
		coeffs[26] = 32'h82a9930;
		coeffs[25] = 32'h85f06f6;
		coeffs[24] = 32'h88ce703;
		coeffs[23] = 32'h8a71de6;
		coeffs[22] = 32'h8bac710;
		coeffs[21] = 32'h8c154c9;
		coeffs[20] = 32'h8bac710;
		coeffs[19] = 32'h8a71de6;
		coeffs[18] = 32'h88ce703;
		coeffs[17] = 32'h85f06f6;
		coeffs[16] = 32'h82a9930;
		coeffs[15] = 32'h7e90ff9;
		coeffs[14] = 32'h7a0f909;
		coeffs[13] = 32'h7525460;
		coeffs[12] = 32'h6f69446;
		coeffs[11] = 32'h6944673;
		coeffs[10] = 32'h631f8a0;
		coeffs[9] = 32'h5c91d14;
		coeffs[8] = 32'h559b3d0;
		coeffs[7] = 32'h4ea4a8c;
		coeffs[6] = 32'h47ae147;
		coeffs[5] = 32'h40b7803;
		coeffs[4] = 32'h39c0ebe;
		coeffs[3] = 32'h32ca57a;
		coeffs[2] = 32'h2ca57a7;
		coeffs[1] = 32'h2617c1b;
		coeffs[0] = 32'h205bc01;
	end
	
	
	// Shift register implementation
	always @(posedge clk) begin
		integer i;
		
		for (i = 42; i > 0; i = i - 1) begin
			memory[i] <= memory[i-1];
		end
		
		memory[0] <= in;
	end

	// Multiply and accumulate
	generate
		genvar i;

		for (i = 0; i < 43; i = i + 1) begin : gen_loop
			assign temp[i] = memory[i] * coeffs[i];
		end
	endgenerate
	
	assign intermediate = (
		temp[0] + temp[1] + temp[2] + temp[3] + temp[4] +
		temp[5] + temp[6] + temp[7] + temp[8] + temp[9] +
		temp[10] + temp[11] + temp[12] + temp[13] + temp[14] +
		temp[15] + temp[16] + temp[17] + temp[18] + temp[19] +
		temp[20] + temp[21] + temp[22] + temp[23] + temp[24] +
		temp[25] + temp[26] + temp[27] + temp[28] + temp[29] +
		temp[30] + temp[31] + temp[32] + temp[33] + temp[34] +
		temp[35] + temp[36] + temp[37] + temp[38] + temp[39] +
		temp[40] + temp[41] + temp[42]
	); 
		
	// Fixed point calculation (convert back)
	assign out = intermediate[63:32];

endmodule
	
