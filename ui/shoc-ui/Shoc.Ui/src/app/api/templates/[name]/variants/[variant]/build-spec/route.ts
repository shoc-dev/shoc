import { NextRequest, NextResponse } from 'next/server';
import { shocClient } from '@/clients/shoc';
import PackageSchemasClient from '@/clients/shoc/package/package-schemas-client';
import axios from 'axios';
import ErrorDefinitions from '@/addons/error-handling/error-definitions';
import TemplatesClient from '@/clients/shoc/package/templates-client';

export async function GET(_: NextRequest, props: { params: Promise<{ name: string, variant: string }> }) {
  const params = await props.params;

  try {
      const { name, variant } = params;
  
      const response = await shocClient(TemplatesClient).getVariantSpec(name, variant);
  
      return NextResponse.json(response.data);
    } catch (error) {

      if (axios.isAxiosError(error) && error.response) {
          return NextResponse.json(error.response.data, { status: error.response.status });
      }

      return NextResponse.json(
        { errors: ErrorDefinitions.unknown('Unexpected error from the server') },
        { status: 500 }
      );
    }
}
